using System;
using System.Globalization;
using System.Text;
using TrueCraft.Nbt.Tags;

namespace TrueCraft.Nbt.Snbt
{
    /// <summary>
    ///     Serialises an <see cref="NbtTag"/> tree to Mojang's Stringified-NBT (SNBT)
    ///     text format. The output round-trips through <see cref="SnbtParser.Parse"/>.
    /// </summary>
    public static class SnbtWriter
    {
        /// <summary>
        ///     Render <paramref name="tag"/> as SNBT.
        /// </summary>
        /// <param name="tag"> The tag to serialise. </param>
        /// <param name="pretty">
        ///     If <c>true</c>, emit a multi-line indented form. If <c>false</c> (default),
        ///     emit compact single-line output matching <c>/data get</c>.
        /// </param>
        public static string ToSnbt(NbtTag tag, bool pretty = false)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            var sb = new StringBuilder();
            WriteTag(sb, tag, pretty, 0);
            return sb.ToString();
        }

        private static void WriteTag(StringBuilder sb, NbtTag tag, bool pretty, int indent)
        {
            switch (tag.TagType)
            {
                case NbtTagType.Byte:
                    sb.Append(((NbtByte) tag).Value).Append('b');
                    break;
                case NbtTagType.Short:
                    sb.Append(((NbtShort) tag).Value).Append('s');
                    break;
                case NbtTagType.Int:
                    sb.Append(((NbtInt) tag).Value);
                    break;
                case NbtTagType.Long:
                    sb.Append(((NbtLong) tag).Value).Append('L');
                    break;
                case NbtTagType.Float:
                    sb.Append(((NbtFloat) tag).Value.ToString("R", CultureInfo.InvariantCulture)).Append('f');
                    break;
                case NbtTagType.Double:
                    sb.Append(((NbtDouble) tag).Value.ToString("R", CultureInfo.InvariantCulture)).Append('d');
                    break;
                case NbtTagType.String:
                    AppendString(sb, ((NbtString) tag).Value);
                    break;
                case NbtTagType.ByteArray:
                    sb.Append("[B;");
                    var bytes = ((NbtByteArray) tag).Value;
                    for (var i = 0; i < bytes.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append((sbyte) bytes[i]).Append('b');
                    }
                    sb.Append(']');
                    break;
                case NbtTagType.IntArray:
                    sb.Append("[I;");
                    var ints = ((NbtIntArray) tag).Value;
                    for (var i = 0; i < ints.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(ints[i]);
                    }
                    sb.Append(']');
                    break;
                case NbtTagType.LongArray:
                    sb.Append("[L;");
                    var longs = ((NbtLongArray) tag).Value;
                    for (var i = 0; i < longs.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(longs[i]).Append('L');
                    }
                    sb.Append(']');
                    break;
                case NbtTagType.List:
                    WriteList(sb, (NbtList) tag, pretty, indent);
                    break;
                case NbtTagType.Compound:
                    WriteCompound(sb, (NbtCompound) tag, pretty, indent);
                    break;
                default:
                    throw new NbtFormatException($"Cannot serialise tag type {tag.TagType} to SNBT");
            }
        }

        private static void WriteList(StringBuilder sb, NbtList list, bool pretty, int indent)
        {
            sb.Append('[');
            if (list.Count == 0)
            {
                sb.Append(']');
                return;
            }
            var first = true;
            foreach (var item in list)
            {
                if (!first) sb.Append(',');
                if (pretty)
                {
                    sb.Append('\n');
                    AppendIndent(sb, indent + 1);
                }
                WriteTag(sb, item, pretty, indent + 1);
                first = false;
            }
            if (pretty)
            {
                sb.Append('\n');
                AppendIndent(sb, indent);
            }
            sb.Append(']');
        }

        private static void WriteCompound(StringBuilder sb, NbtCompound compound, bool pretty, int indent)
        {
            sb.Append('{');
            if (compound.Count == 0)
            {
                sb.Append('}');
                return;
            }
            var first = true;
            foreach (var tag in compound)
            {
                if (!first) sb.Append(',');
                if (pretty)
                {
                    sb.Append('\n');
                    AppendIndent(sb, indent + 1);
                }
                AppendKey(sb, tag.Name);
                sb.Append(':');
                if (pretty) sb.Append(' ');
                WriteTag(sb, tag, pretty, indent + 1);
                first = false;
            }
            if (pretty)
            {
                sb.Append('\n');
                AppendIndent(sb, indent);
            }
            sb.Append('}');
        }

        private static void AppendKey(StringBuilder sb, string key)
        {
            if (IsUnquotedKey(key))
                sb.Append(key);
            else
                AppendString(sb, key);
        }

        private static bool IsUnquotedKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            foreach (var c in key)
            {
                var ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                         (c >= '0' && c <= '9') || c == '_' || c == '+' || c == '-' || c == '.';
                if (!ok) return false;
            }
            return true;
        }

        private static void AppendString(StringBuilder sb, string value)
        {
            // Mojang prefers " unless the string contains " and not ', in which case use '.
            var quote = '"';
            if (value.IndexOf('"') >= 0 && value.IndexOf('\'') < 0)
                quote = '\'';

            sb.Append(quote);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c == quote)
                        {
                            sb.Append('\\').Append(c);
                        }
                        else if (c < 0x20)
                        {
                            sb.Append("\\u").Append(((int) c).ToString("X4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append(quote);
        }

        private static void AppendIndent(StringBuilder sb, int level)
        {
            for (var i = 0; i < level; i++) sb.Append("  ");
        }
    }
}
