using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TrueCraft.Nbt;

/// <summary>
///     BinaryReader wrapper that takes care of reading primitives from an NBT stream,
///     while taking care of endianness, string encoding, and skipping.
/// </summary>
internal sealed class NbtBinaryReader : BinaryReader
{
    private const int SeekBufferSize = 8 * 1024;
    private readonly byte[] _buffer = new byte[sizeof(double)];
    private readonly byte[] _stringConversionBuffer = new byte[64];
    private readonly bool _swapNeeded;
    private readonly bool _bigEndian;

    private byte[] _seekBuffer;


    public NbtBinaryReader([NotNull] Stream input, bool bigEndian)
        : base(input)
    {
        _swapNeeded = BitConverter.IsLittleEndian == bigEndian;
        _bigEndian = bigEndian;
    }


    [CanBeNull] public TagSelector Selector { get; set; }

    /// <summary>
    ///     When <c>true</c>, strings are decoded as standard UTF-8 instead of Java's
    ///     Modified UTF-8. Used by the network-NBT framing (Java protocol 1.20.2+).
    /// </summary>
    public bool UseStandardUtf8 { get; set; }

    /// <summary>
    ///     Maximum allowed nesting depth of compounds and lists during a recursive
    ///     read. Defaults to 512 to match Mojang's parser limit. Set higher to allow
    ///     deeper structures; set to 0 to disable.
    /// </summary>
    public int MaxDepth { get; set; } = 512;

    /// <summary> Current recursion depth — incremented on entry into a compound or list. </summary>
    public int Depth { get; private set; }

    /// <summary>
    ///     Maximum allowed element count for any single TAG_Byte_Array, TAG_Int_Array,
    ///     TAG_Long_Array, or TAG_List payload. Adversarial NBT can declare an array
    ///     length of <c>int.MaxValue</c>, which would attempt to allocate up to 16 GiB
    ///     for a TAG_Long_Array. Defaults to 16 * 1024 * 1024 (~16 million elements).
    ///     Set to 0 to disable.
    /// </summary>
    public int MaxArrayElements { get; set; } = 16 * 1024 * 1024;

    /// <summary>
    ///     Validate an on-the-wire array length: reject negative values, reject
    ///     allocations beyond <see cref="MaxArrayElements"/>.
    /// </summary>
    public void CheckArrayLength(int length, string tagName)
    {
        if (length < 0)
            throw new NbtFormatException(
                $"Negative length given in {tagName}: {length}");
        if (MaxArrayElements > 0 && length > MaxArrayElements)
            throw new NbtFormatException(
                $"Array length {length} for {tagName} exceeds MaxArrayElements = {MaxArrayElements}");
    }

    /// <summary>
    ///     Increment recursion depth; throws <see cref="NbtFormatException"/> if the
    ///     configured <see cref="MaxDepth"/> is exceeded.
    /// </summary>
    public void EnterNested()
    {
        Depth++;
        if (MaxDepth > 0 && Depth > MaxDepth)
            throw new NbtFormatException(
                $"NBT nesting depth exceeded MaxDepth = {MaxDepth}");
    }

    /// <summary> Decrement recursion depth at the matching end of a compound or list. </summary>
    public void ExitNested()
    {
        if (Depth > 0) Depth--;
    }


    public NbtTagType ReadTagType()
    {
        int type = ReadByte();
        if (type < 0) throw new EndOfStreamException();

        if (type > (int) NbtTagType.LongArray) throw new NbtFormatException("NBT tag type out of range: " + type);

        return (NbtTagType) type;
    }


    public override short ReadInt16()
    {
        if (_swapNeeded) return Swap(base.ReadInt16());

        return base.ReadInt16();
    }


    public override int ReadInt32()
    {
        if (_swapNeeded) return Swap(base.ReadInt32());

        return base.ReadInt32();
    }


    public override long ReadInt64()
    {
        if (_swapNeeded) return Swap(base.ReadInt64());

        return base.ReadInt64();
    }


    public override float ReadSingle()
    {
        if (_swapNeeded)
        {
            FillBuffer(sizeof(float));
            Array.Reverse(_buffer, 0, sizeof(float));
            return BitConverter.ToSingle(_buffer, 0);
        }

        return base.ReadSingle();
    }


    public override double ReadDouble()
    {
        if (_swapNeeded)
        {
            FillBuffer(sizeof(double));
            Array.Reverse(_buffer);
            return BitConverter.ToDouble(_buffer, 0);
        }

        return base.ReadDouble();
    }


    public override string ReadString()
    {
        // NBT string length prefix is unsigned 16-bit per spec. Read as unsigned to
        // accept the full 0..65535 range Mojang's writer can produce.
        var length = ReadUInt16Spec();

        byte[] stringData;
        if (length <= _stringConversionBuffer.Length)
        {
            var stringBytesRead = 0;
            while (stringBytesRead < length)
            {
                var bytesToRead = length - stringBytesRead;
                var bytesReadThisTime = BaseStream.Read(_stringConversionBuffer, stringBytesRead, bytesToRead);
                if (bytesReadThisTime == 0) throw new EndOfStreamException();
                stringBytesRead += bytesReadThisTime;
            }
            stringData = _stringConversionBuffer;
        }
        else
        {
            stringData = ReadBytes(length);
            if (stringData.Length < length) throw new EndOfStreamException();
        }

        return UseStandardUtf8
            ? Encoding.UTF8.GetString(stringData, 0, length)
            : JavaModifiedUtf8.Decode(stringData, 0, length);
    }


    // Read an unsigned 16-bit length prefix in the configured wire endianness.
    // The base BinaryReader.ReadUInt16 is hard-coded little-endian; we want the
    // big-endian wire form for vanilla Java NBT (and little-endian for Bedrock).
    private ushort ReadUInt16Spec()
    {
        int a = BaseStream.ReadByte();
        int b = BaseStream.ReadByte();
        if (a < 0 || b < 0) throw new EndOfStreamException();
        // Wire big-endian: a is MSB. Wire little-endian: a is LSB.
        return _bigEndian
            ? (ushort) ((a << 8) | b)
            : (ushort) ((b << 8) | a);
    }


    public void Skip(int bytesToSkip)
    {
        if (bytesToSkip < 0) throw new ArgumentOutOfRangeException(nameof(bytesToSkip));

        if (BaseStream.CanSeek)
        {
            BaseStream.Position += bytesToSkip;
        }
        else if (bytesToSkip != 0)
        {
            if (_seekBuffer is null) _seekBuffer = new byte[SeekBufferSize];
            var bytesSkipped = 0;
            while (bytesSkipped < bytesToSkip)
            {
                var bytesToRead = Math.Min(SeekBufferSize, bytesToSkip - bytesSkipped);
                var bytesReadThisTime = BaseStream.Read(_seekBuffer, 0, bytesToRead);
                if (bytesReadThisTime == 0) throw new EndOfStreamException();

                bytesSkipped += bytesReadThisTime;
            }
        }
    }


    private new void FillBuffer(int numBytes)
    {
        var offset = 0;
        do
        {
            var num = BaseStream.Read(_buffer, offset, numBytes - offset);
            if (num == 0) throw new EndOfStreamException();
            offset += num;
        } while (offset < numBytes);
    }


    public void SkipString()
    {
        var length = ReadUInt16Spec();
        Skip(length);
    }


    [DebuggerStepThrough]
    private static short Swap(short v)
    {
        unchecked
        {
            return (short) (((v >> 8) & 0x00FF) |
                            ((v << 8) & 0xFF00));
        }
    }


    [DebuggerStepThrough]
    private static int Swap(int v)
    {
        unchecked
        {
            var v2 = (uint) v;
            return (int) (((v2 >> 24) & 0x000000FF) |
                          ((v2 >> 8) & 0x0000FF00) |
                          ((v2 << 8) & 0x00FF0000) |
                          ((v2 << 24) & 0xFF000000));
        }
    }


    [DebuggerStepThrough]
    private static long Swap(long v)
    {
        unchecked
        {
            return ((Swap((int) v) & uint.MaxValue) << 32) |
                   (Swap((int) (v >> 32)) & uint.MaxValue);
        }
    }
}
