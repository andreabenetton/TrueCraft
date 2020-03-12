using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrueCraft.Core.Networking
{
    public class ByteListMemoryStream : Stream
    {
        private readonly List<byte> buffer;
        private long position;

        public ByteListMemoryStream() : this(new List<byte>())
        {
        }

        public ByteListMemoryStream(List<byte> buffer, int offset = 0)
        {
            position = offset;
            this.buffer = buffer;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => buffer.Count;

        public override long Position
        {
            get => position;
            set => position = value;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                position = offset;
            else if (origin == SeekOrigin.Current)
                position += offset;
            else //End
                position = buffer.Count - 1 - offset;

            return position;
        }

        public override void SetLength(long value)
        {
            buffer.RemoveRange((int) value, buffer.Count - (int) value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset)
                throw new ArgumentOutOfRangeException("offset");

            if (buffer.Length < count)
                throw new ArgumentOutOfRangeException("count");

            var buf = this.buffer.Skip((int) position).Take(count).ToArray();

            Buffer.BlockCopy(buf, 0, buffer, offset, buf.Length);

            position += Math.Min(count, buf.Length);

            return Math.Min(count, buf.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset)
                throw new ArgumentOutOfRangeException("offset");

            if (buffer.Length < count)
                throw new ArgumentOutOfRangeException("count");

            this.buffer.AddRange(buffer.Skip(offset).Take(count));
            position += count;
        }
    }
}