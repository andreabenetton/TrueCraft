using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace TrueCraft.Nbt {
    /// <summary> DeflateStream wrapper that calculates Adler32 checksum of the written data,
    /// to allow writing ZLib header (RFC-1950). </summary>
    internal sealed class ZLibStream : DeflateStream {
        int _adler32A = 1,
            _adler32B;

        const int ChecksumModulus = 65521;

        public int Checksum => unchecked((_adler32B*65536) + _adler32A);


        void UpdateChecksum([NotNull] IList<byte> data, int offset, int length) {
            for (int counter = 0; counter < length; ++counter) {
                _adler32A = (_adler32A + (data[offset + counter]))%ChecksumModulus;
                _adler32B = (_adler32B + _adler32A)%ChecksumModulus;
            }
        }


        public ZLibStream([NotNull] Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {}


        public override void Write(byte[] array, int offset, int count) {
            UpdateChecksum(array, offset, count);
            base.Write(array, offset, count);
        }
    }
}
