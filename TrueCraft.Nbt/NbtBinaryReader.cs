using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TrueCraft.Nbt {
    /// <summary> BinaryReader wrapper that takes care of reading primitives from an NBT stream,
    /// while taking care of endianness, string encoding, and skipping. </summary>
    internal sealed class NbtBinaryReader : BinaryReader {
        private readonly byte[] _buffer = new byte[sizeof(double)];

        private byte[] _seekBuffer;
        private const int SeekBufferSize = 8*1024;
        private readonly bool _swapNeeded;
        private readonly byte[] _stringConversionBuffer = new byte[64];


        public NbtBinaryReader([NotNull] Stream input, bool bigEndian)
            : base(input) {
            _swapNeeded = (BitConverter.IsLittleEndian == bigEndian);
        }


        public NbtTagType ReadTagType() {
            int type = ReadByte();
            if (type < 0) {
                throw new EndOfStreamException();
            }

            if (type > (int)NbtTagType.IntArray) {
                throw new NbtFormatException("NBT tag type out of range: " + type);
            }
            return (NbtTagType)type;
        }


        public override short ReadInt16()
        {
            if (_swapNeeded) {
                return Swap(base.ReadInt16());
            }

            return base.ReadInt16();
        }


        public override int ReadInt32()
        {
            if (_swapNeeded) {
                return Swap(base.ReadInt32());
            }

            return base.ReadInt32();
        }


        public override long ReadInt64()
        {
            if (_swapNeeded) {
                return Swap(base.ReadInt64());
            }

            return base.ReadInt64();
        }


        public override float ReadSingle()
        {
            if (_swapNeeded) {
                FillBuffer(sizeof(float));
                Array.Reverse(_buffer, 0, sizeof(float));
                return BitConverter.ToSingle(_buffer, 0);
            }

            return base.ReadSingle();
        }


        public override double ReadDouble() {
            if (_swapNeeded) {
                FillBuffer(sizeof(double));
                Array.Reverse(_buffer);
                return BitConverter.ToDouble(_buffer, 0);
            }
            return base.ReadDouble();
        }


        public override string ReadString() {
            short length = ReadInt16();
            if (length < 0) {
                throw new NbtFormatException("Negative string length given!");
            }
            if (length < _stringConversionBuffer.Length) {
                int stringBytesRead = 0;
                while (stringBytesRead < length) {
                    int bytesToRead = length - stringBytesRead;
                    int bytesReadThisTime = BaseStream.Read(_stringConversionBuffer, stringBytesRead, bytesToRead);
                    if (bytesReadThisTime == 0) {
                        throw new EndOfStreamException();
                    }
                    stringBytesRead += bytesReadThisTime;
                }
                return Encoding.UTF8.GetString(_stringConversionBuffer, 0, length);
            }

            byte[] stringData = ReadBytes(length);
            if (stringData.Length < length) {
                throw new EndOfStreamException();
            }
            return Encoding.UTF8.GetString(stringData);
        }


        public void Skip(int bytesToSkip)
        {
            if (bytesToSkip < 0) {
                throw new ArgumentOutOfRangeException(nameof(bytesToSkip));
            }

            if (BaseStream.CanSeek) {
                BaseStream.Position += bytesToSkip;
            } else if (bytesToSkip != 0) {
                if (_seekBuffer == null) _seekBuffer = new byte[SeekBufferSize];
                int bytesSkipped = 0;
                while (bytesSkipped < bytesToSkip) {
                    int bytesToRead = Math.Min(SeekBufferSize, bytesToSkip - bytesSkipped);
                    int bytesReadThisTime = BaseStream.Read(_seekBuffer, 0, bytesToRead);
                    if (bytesReadThisTime == 0) {
                        throw new EndOfStreamException();
                    }
                    bytesSkipped += bytesReadThisTime;
                }
            }
        }


        private new void FillBuffer(int numBytes) {
            int offset = 0;
            do {
                int num = BaseStream.Read(_buffer, offset, numBytes - offset);
                if (num == 0) throw new EndOfStreamException();
                offset += num;
            } while (offset < numBytes);
        }


        public void SkipString() {
            short length = ReadInt16();
            if (length < 0) {
                throw new NbtFormatException("Negative string length given!");
            }
            Skip(length);
        }


        [DebuggerStepThrough]
        private static short Swap(short v) {
            unchecked {
                return (short)((v >> 8) & 0x00FF |
                               (v << 8) & 0xFF00);
            }
        }


        [DebuggerStepThrough]
        private static int Swap(int v) {
            unchecked {
                var v2 = (uint)v;
                return (int)((v2 >> 24) & 0x000000FF |
                             (v2 >> 8) & 0x0000FF00 |
                             (v2 << 8) & 0x00FF0000 |
                             (v2 << 24) & 0xFF000000);
            }
        }


        [DebuggerStepThrough]
        private static long Swap(long v) {
            unchecked {
                return (Swap((int)v) & uint.MaxValue) << 32 |
                       Swap((int)(v >> 32)) & uint.MaxValue;
            }
        }


        [CanBeNull]
        public TagSelector Selector { get; set; }
    }
}
