using System;
using System.IO;
using System.Text;

namespace TrueCraft.Nbt
{
    /// <summary>
    ///     BinaryWriter wrapper that writes NBT primitives to a stream,
    ///     while taking care of endianness and string encoding, and counting bytes written.
    /// </summary>
    internal sealed unsafe class NbtBinaryWriter
    {
        // Write at most 4 MiB at a time.
        public const int MAX_WRITE_CHUNK = 4 * 1024 * 1024;

        // Buffer used for temporary conversion
        private const int BufferSize = 256;

        // UTF8 characters use at most 4 bytes each.
        private const int MaxBufferedStringLength = BufferSize / 4;

        // Encoding can be shared among all instances of NbtBinaryWriter, because it is stateless.
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);

        // Each NbtBinaryWriter needs to have its own instance of the buffer.
        private readonly byte[] _buffer = new byte[BufferSize];

        // Each instance has to have its own encoder, because it does maintain state.
        private readonly Encoder _encoder = Encoding.GetEncoder();

        private readonly Stream _stream;

        // Swap is only needed if endianness of the runtime differs from desired NBT stream
        private readonly bool _swapNeeded;


        public NbtBinaryWriter([NotNull] Stream input, bool bigEndian)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (!input.CanWrite) throw new ArgumentException("Given stream must be writable", nameof(input));
            _stream = input;
            _swapNeeded = BitConverter.IsLittleEndian == bigEndian;
            _bigEndian = bigEndian;
        }

        /// <summary>
        ///     When <c>true</c>, strings are encoded as standard UTF-8 instead of Java's
        ///     Modified UTF-8. Used by the network-NBT framing (Java protocol 1.20.2+).
        /// </summary>
        public bool UseStandardUtf8 { get; set; }

        private readonly bool _bigEndian;

        public Stream BaseStream
        {
            get
            {
                _stream.Flush();
                return _stream;
            }
        }


        public void Write(byte value)
        {
            _stream.WriteByte(value);
        }


        public void Write(NbtTagType value)
        {
            _stream.WriteByte((byte) value);
        }


        public void Write(short value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte) (value >> 8);
                    _buffer[1] = (byte) value;
                }
                else
                {
                    _buffer[0] = (byte) value;
                    _buffer[1] = (byte) (value >> 8);
                }
            }

            _stream.Write(_buffer, 0, 2);
        }


        public void Write(int value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte) (value >> 24);
                    _buffer[1] = (byte) (value >> 16);
                    _buffer[2] = (byte) (value >> 8);
                    _buffer[3] = (byte) value;
                }
                else
                {
                    _buffer[0] = (byte) value;
                    _buffer[1] = (byte) (value >> 8);
                    _buffer[2] = (byte) (value >> 16);
                    _buffer[3] = (byte) (value >> 24);
                }
            }

            _stream.Write(_buffer, 0, 4);
        }


        public void Write(long value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte) (value >> 56);
                    _buffer[1] = (byte) (value >> 48);
                    _buffer[2] = (byte) (value >> 40);
                    _buffer[3] = (byte) (value >> 32);
                    _buffer[4] = (byte) (value >> 24);
                    _buffer[5] = (byte) (value >> 16);
                    _buffer[6] = (byte) (value >> 8);
                    _buffer[7] = (byte) value;
                }
                else
                {
                    _buffer[0] = (byte) value;
                    _buffer[1] = (byte) (value >> 8);
                    _buffer[2] = (byte) (value >> 16);
                    _buffer[3] = (byte) (value >> 24);
                    _buffer[4] = (byte) (value >> 32);
                    _buffer[5] = (byte) (value >> 40);
                    _buffer[6] = (byte) (value >> 48);
                    _buffer[7] = (byte) (value >> 56);
                }
            }

            _stream.Write(_buffer, 0, 8);
        }


        public void Write(float value)
        {
            ulong tmpValue = *(uint*) &value;
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte) (tmpValue >> 24);
                    _buffer[1] = (byte) (tmpValue >> 16);
                    _buffer[2] = (byte) (tmpValue >> 8);
                    _buffer[3] = (byte) tmpValue;
                }
                else
                {
                    _buffer[0] = (byte) tmpValue;
                    _buffer[1] = (byte) (tmpValue >> 8);
                    _buffer[2] = (byte) (tmpValue >> 16);
                    _buffer[3] = (byte) (tmpValue >> 24);
                }
            }

            _stream.Write(_buffer, 0, 4);
        }


        public void Write(double value)
        {
            var tmpValue = *(ulong*) &value;
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte) (tmpValue >> 56);
                    _buffer[1] = (byte) (tmpValue >> 48);
                    _buffer[2] = (byte) (tmpValue >> 40);
                    _buffer[3] = (byte) (tmpValue >> 32);
                    _buffer[4] = (byte) (tmpValue >> 24);
                    _buffer[5] = (byte) (tmpValue >> 16);
                    _buffer[6] = (byte) (tmpValue >> 8);
                    _buffer[7] = (byte) tmpValue;
                }
                else
                {
                    _buffer[0] = (byte) tmpValue;
                    _buffer[1] = (byte) (tmpValue >> 8);
                    _buffer[2] = (byte) (tmpValue >> 16);
                    _buffer[3] = (byte) (tmpValue >> 24);
                    _buffer[4] = (byte) (tmpValue >> 32);
                    _buffer[5] = (byte) (tmpValue >> 40);
                    _buffer[6] = (byte) (tmpValue >> 48);
                    _buffer[7] = (byte) (tmpValue >> 56);
                }
            }

            _stream.Write(_buffer, 0, 8);
        }


        // Strings on the wire are Java Modified UTF-8 by default (used by every
        // disk-format NBT Mojang has written). Network NBT (Java 1.20.2+ protocol) uses
        // standard UTF-8 — opt in via UseStandardUtf8.
        public void Write([NotNull] string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            int numBytes;
            byte[] bytes;
            if (UseStandardUtf8)
            {
                numBytes = Encoding.GetByteCount(value);
                bytes = null; // populated below if needed
            }
            else
            {
                numBytes = JavaModifiedUtf8.GetByteCount(value);
                bytes = null;
            }
            if (numBytes > ushort.MaxValue)
                throw new NbtFormatException(
                    $"NBT string longer than {ushort.MaxValue} bytes ({numBytes} bytes)");

            WriteUInt16Spec((ushort) numBytes);

            if (numBytes <= BufferSize)
            {
                int written;
                if (UseStandardUtf8)
                    written = Encoding.GetBytes(value, 0, value.Length, _buffer, 0);
                else
                    written = JavaModifiedUtf8.Encode(value, _buffer, 0);
                _stream.Write(_buffer, 0, written);
            }
            else
            {
                // Single allocation for the whole payload — sized exactly via GetByteCount.
                bytes = new byte[numBytes];
                if (UseStandardUtf8)
                    Encoding.GetBytes(value, 0, value.Length, bytes, 0);
                else
                    JavaModifiedUtf8.Encode(value, bytes, 0);
                _stream.Write(bytes, 0, numBytes);
            }
        }


        // Write an unsigned 16-bit length prefix in the configured wire endianness.
        private void WriteUInt16Spec(ushort value)
        {
            if (_bigEndian)
            {
                _buffer[0] = (byte) (value >> 8);
                _buffer[1] = (byte) value;
            }
            else
            {
                _buffer[0] = (byte) value;
                _buffer[1] = (byte) (value >> 8);
            }
            _stream.Write(_buffer, 0, 2);
        }


        public void Write(byte[] data, int offset, int count)
        {
            var written = 0;
            while (written < count)
            {
                var toWrite = Math.Min(MAX_WRITE_CHUNK, count - written);
                _stream.Write(data, offset + written, toWrite);
                written += toWrite;
            }
        }
    }
}