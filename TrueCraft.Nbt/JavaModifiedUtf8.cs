using System;
using System.IO;

namespace TrueCraft.Nbt
{
    /// <summary>
    ///     Encodes and decodes strings in Java's "Modified UTF-8" — the encoding used by
    ///     <c>DataInputStream.readUTF</c> / <c>DataOutputStream.writeUTF</c> and therefore
    ///     by every NBT file Mojang's Java edition ever wrote.
    ///
    ///     <para>
    ///     Modified UTF-8 differs from standard UTF-8 in exactly two ways:
    ///     <list type="bullet">
    ///         <item>
    ///             The code point U+0000 is encoded as the overlong two-byte sequence
    ///             <c>0xC0 0x80</c> instead of a single <c>0x00</c> byte, so that NUL
    ///             can appear inside C-style null-terminated strings without truncation.
    ///         </item>
    ///         <item>
    ///             Code points in the supplementary plane (U+10000..U+10FFFF) are encoded
    ///             as two separate three-byte CESU-8 surrogate halves (six bytes total)
    ///             rather than as a single four-byte UTF-8 sequence.
    ///         </item>
    ///     </list>
    ///     A plain <c>Encoding.UTF8</c> codec produces files that Mojang's parser rejects
    ///     for any string containing NUL or a supplementary character (most emoji), and
    ///     fails to read files Mojang's writer produces for the same reason.
    ///     </para>
    ///
    ///     <para>
    ///     This class operates on <see cref="char"/> arrays / spans and preserves lone
    ///     unpaired UTF-16 surrogates by encoding them as their 3-byte CESU-8 form, the
    ///     same lossy round-trip Java uses.
    ///     </para>
    ///
    ///     <para>Reference: minecraft.wiki/w/NBT_format and JDK <c>DataInput</c> spec.</para>
    /// </summary>
    public static class JavaModifiedUtf8
    {
        /// <summary>
        ///     Returns the number of bytes required to encode <paramref name="value"/>
        ///     in Modified UTF-8.
        /// </summary>
        public static int GetByteCount(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var bytes = 0;
            foreach (var c in value)
            {
                if (c >= 0x0001 && c <= 0x007F) bytes += 1;
                else if (c <= 0x07FF) bytes += 2;      // includes U+0000 (encoded as C0 80)
                else bytes += 3;                       // U+0800..U+FFFF, including surrogates
            }
            return bytes;
        }

        /// <summary>
        ///     Encodes <paramref name="value"/> into <paramref name="destination"/> starting
        ///     at <paramref name="offset"/>. Returns the number of bytes written.
        /// </summary>
        public static int Encode(string value, byte[] destination, int offset)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            var pos = offset;
            foreach (var c in value)
            {
                if (c >= 0x0001 && c <= 0x007F)
                {
                    destination[pos++] = (byte) c;
                }
                else if (c <= 0x07FF)
                {
                    destination[pos++] = (byte) (0xC0 | (c >> 6));
                    destination[pos++] = (byte) (0x80 | (c & 0x3F));
                }
                else
                {
                    destination[pos++] = (byte) (0xE0 | (c >> 12));
                    destination[pos++] = (byte) (0x80 | ((c >> 6) & 0x3F));
                    destination[pos++] = (byte) (0x80 | (c & 0x3F));
                }
            }
            return pos - offset;
        }

        /// <summary>
        ///     Returns a freshly-allocated byte array containing the Modified UTF-8 form
        ///     of <paramref name="value"/>.
        /// </summary>
        public static byte[] Encode(string value)
        {
            var buf = new byte[GetByteCount(value)];
            Encode(value, buf, 0);
            return buf;
        }

        /// <summary>
        ///     Decodes <paramref name="length"/> Modified UTF-8 bytes starting at
        ///     <paramref name="offset"/> in <paramref name="source"/> into a string.
        /// </summary>
        /// <exception cref="NbtFormatException">
        ///     The byte sequence is malformed — it contains a 4+-byte UTF-8 sequence,
        ///     an unexpected continuation byte, or runs past <paramref name="length"/>.
        /// </exception>
        public static string Decode(byte[] source, int offset, int length)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (offset < 0 || length < 0 || offset + length > source.Length)
                throw new ArgumentOutOfRangeException();

            var chars = new char[length]; // worst case: 1 byte per char (ASCII)
            var charPos = 0;
            var end = offset + length;
            var pos = offset;
            while (pos < end)
            {
                int b = source[pos++];
                int c;
                if ((b & 0x80) == 0)
                {
                    // 0xxxxxxx — single byte ASCII (U+0001..U+007F).
                    c = b;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    // 110xxxxx 10xxxxxx — two bytes (U+0000 via overlong, or U+0080..U+07FF).
                    if (pos >= end) throw new NbtFormatException("Truncated Modified UTF-8 sequence");
                    int b2 = source[pos++];
                    if ((b2 & 0xC0) != 0x80)
                        throw new NbtFormatException("Bad continuation byte in Modified UTF-8 sequence");
                    c = ((b & 0x1F) << 6) | (b2 & 0x3F);
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    // 1110xxxx 10xxxxxx 10xxxxxx — three bytes (U+0800..U+FFFF, including surrogates).
                    if (pos + 1 >= end) throw new NbtFormatException("Truncated Modified UTF-8 sequence");
                    int b2 = source[pos++];
                    int b3 = source[pos++];
                    if ((b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80)
                        throw new NbtFormatException("Bad continuation byte in Modified UTF-8 sequence");
                    c = ((b & 0x0F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F);
                }
                else
                {
                    // 4-byte UTF-8 sequence (0xF0..0xF7 lead) is NOT valid in Modified UTF-8.
                    // Supplementary characters are split into two 3-byte CESU-8 surrogate halves.
                    throw new NbtFormatException(
                        $"Invalid Modified UTF-8 lead byte 0x{b:X2}: standard 4-byte UTF-8 not allowed.");
                }

                chars[charPos++] = (char) c;
            }

            return new string(chars, 0, charPos);
        }

        /// <summary>
        ///     Convenience: decode the whole array.
        /// </summary>
        public static string Decode(byte[] source) => Decode(source, 0, source?.Length ?? 0);

        /// <summary>
        ///     Encode then write to the given stream. No length prefix; the caller is
        ///     responsible for framing.
        /// </summary>
        public static void Write(Stream destination, string value)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            var bytes = Encode(value);
            destination.Write(bytes, 0, bytes.Length);
        }
    }
}
