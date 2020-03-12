using System.IO;

namespace TrueCraft.Core.Networking
{
    /// <summary>
    ///     Queues all writes until Stream.Flush() is called. This is different than System.IO.BufferedStream.
    /// </summary>
    public class BufferedStream : Stream
    {
        public BufferedStream(Stream baseStream)
        {
            BaseStream = baseStream;
            PendingStream = new MemoryStream(512);
            WriteImmediately = false;
        }

        public Stream BaseStream { get; set; }
        public MemoryStream PendingStream { get; set; }

        /// <summary>
        ///     Used by PacketReader to insert the ID and length into the stream before the packet contents.
        /// </summary>
        internal bool WriteImmediately { get; set; }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public long PendingWrites => PendingStream.Position;

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override void Flush()
        {
            BaseStream.Write(PendingStream.GetBuffer(), 0, (int) PendingStream.Position);
            PendingStream.Position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (WriteImmediately)
                BaseStream.Write(buffer, offset, count);
            else
                PendingStream.Write(buffer, offset, count);
        }
    }
}