using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TrueCraft.Nbt.Tags;

namespace TrueCraft.Nbt.Snbt;

/// <summary>
///     Parses Mojang's Stringified-NBT (SNBT) text format into an
///     <see cref="NbtTag"/> tree. SNBT is the format used in commands, datapacks,
///     and the <c>/data get</c> output.
///
///     <para>Grammar highlights:</para>
///     <list type="bullet">
///         <item>Compounds use <c>{ key: value, ... }</c>. Keys may be unquoted
///         if they match <c>[A-Za-z0-9_+\-.]+</c> and don't start with a digit;
///         otherwise quote them with single or double quotes.</item>
///         <item>Lists use <c>[value, value, ...]</c>. Element type is inferred
///         from the first element; mixed types are a parse error.</item>
///         <item>Typed arrays use a type prefix: <c>[B; 1b, 2b]</c> for byte,
///         <c>[I; 1, 2]</c> for int, <c>[L; 1L, 2L]</c> for long.</item>
///         <item>Numeric suffixes select the tag type: <c>b/B</c> byte, <c>s/S</c>
///         short, no suffix int, <c>l/L</c> long, <c>f/F</c> float, <c>d/D</c>
///         double. Unsuffixed decimals are double.</item>
///         <item>Strings: <c>"..."</c> or <c>'...'</c>, with <c>\\ \" \' \n \t</c>
///         and <c>\u####</c> escapes.</item>
///         <item>Booleans: <c>true</c>/<c>false</c> -&gt; NbtByte 1/0.</item>
///         <item>Trailing commas in compounds/lists are accepted.</item>
///     </list>
/// </summary>
public static class SnbtParser
{
    /// <summary>
    ///     Parse <paramref name="snbt"/> into an <see cref="NbtTag"/> tree.
    ///     Throws <see cref="SnbtParseException"/> on any grammar violation.
    /// </summary>
    public static NbtTag Parse(string snbt)
    {
        if (snbt is null) throw new ArgumentNullException(nameof(snbt));
        var p = new Parser(snbt);
        p.SkipWhitespace();
        var tag = p.ParseValue();
        p.SkipWhitespace();
        if (p.HasMore)
            throw new SnbtParseException("Unexpected trailing input", p.Pos);
        return tag;
    }

    private struct Parser
    {
        private readonly string _src;
        public int Pos;

        public Parser(string src)
        {
            _src = src;
            Pos = 0;
        }

        public bool HasMore => Pos < _src.Length;
        private char Peek => _src[Pos];

        public void SkipWhitespace()
        {
            while (Pos < _src.Length && char.IsWhiteSpace(_src[Pos])) Pos++;
        }

        public NbtTag ParseValue()
        {
            SkipWhitespace();
            if (!HasMore) throw new SnbtParseException("Expected a value", Pos);
            var c = Peek;
            if (c == '{') return ParseCompound();
            if (c == '[') return ParseListOrArray();
            if (c == '"' || c == '\'') return new NbtString(ParseQuotedString());
            return ParseScalar();
        }

        private NbtTag ParseCompound()
        {
            Expect('{');
            var compound = new NbtCompound();
            SkipWhitespace();
            while (HasMore && Peek != '}')
            {
                SkipWhitespace();
                var name = ParseKey();
                SkipWhitespace();
                Expect(':');
                var value = ParseValue();
                value.Name = name;
                compound.Add(value);
                SkipWhitespace();
                if (HasMore && Peek == ',')
                {
                    Pos++;
                    SkipWhitespace();
                }
                else
                {
                    break;
                }
            }
            SkipWhitespace();
            Expect('}');
            return compound;
        }

        private string ParseKey()
        {
            SkipWhitespace();
            if (!HasMore) throw new SnbtParseException("Expected key", Pos);
            if (Peek == '"' || Peek == '\'') return ParseQuotedString();
            var start = Pos;
            while (Pos < _src.Length && IsUnquotedKeyChar(_src[Pos])) Pos++;
            if (Pos == start) throw new SnbtParseException("Expected key", start);
            return _src.Substring(start, Pos - start);
        }

        private static bool IsUnquotedKeyChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                   (c >= '0' && c <= '9') || c == '_' || c == '+' || c == '-' || c == '.';
        }

        private NbtTag ParseListOrArray()
        {
            Expect('[');
            if (Pos + 1 < _src.Length && _src[Pos + 1] == ';')
            {
                var prefix = _src[Pos];
                if (prefix == 'B' || prefix == 'I' || prefix == 'L')
                {
                    Pos += 2;
                    return ParseTypedArray(prefix);
                }
            }
            return ParseList();
        }

        private NbtTag ParseList()
        {
            var list = new NbtList();
            SkipWhitespace();
            while (HasMore && Peek != ']')
            {
                SkipWhitespace();
                var element = ParseValue();
                if (list.Count == 0)
                    list.ListType = element.TagType;
                else if (element.TagType != list.ListType)
                    throw new SnbtParseException(
                        $"List element has type {element.TagType} but list is {list.ListType}", Pos);
                list.Add(element);
                SkipWhitespace();
                if (HasMore && Peek == ',')
                {
                    Pos++;
                    SkipWhitespace();
                }
                else
                {
                    break;
                }
            }
            SkipWhitespace();
            Expect(']');
            return list;
        }

        private NbtTag ParseTypedArray(char prefix)
        {
            SkipWhitespace();
            if (prefix == 'B')
            {
                var values = new List<byte>();
                while (HasMore && Peek != ']')
                {
                    SkipWhitespace();
                    var t = ParseScalar();
                    if (t is NbtByte nb) values.Add((byte) nb.Value);
                    else throw new SnbtParseException($"Byte array element must be byte; got {t.TagType}", Pos);
                    SkipWhitespace();
                    if (HasMore && Peek == ',') { Pos++; SkipWhitespace(); }
                    else break;
                }
                SkipWhitespace();
                Expect(']');
                var arr = new byte[values.Count];
                for (var i = 0; i < values.Count; i++) arr[i] = values[i];
                return new NbtByteArray(arr);
            }
            if (prefix == 'I')
            {
                var values = new List<int>();
                while (HasMore && Peek != ']')
                {
                    SkipWhitespace();
                    var t = ParseScalar();
                    if (t is NbtInt ni) values.Add(ni.Value);
                    else throw new SnbtParseException($"Int array element must be int; got {t.TagType}", Pos);
                    SkipWhitespace();
                    if (HasMore && Peek == ',') { Pos++; SkipWhitespace(); }
                    else break;
                }
                SkipWhitespace();
                Expect(']');
                return new NbtIntArray(values.ToArray());
            }
            // 'L'
            {
                var values = new List<long>();
                while (HasMore && Peek != ']')
                {
                    SkipWhitespace();
                    var t = ParseScalar();
                    if (t is NbtLong nl) values.Add(nl.Value);
                    else throw new SnbtParseException($"Long array element must be long; got {t.TagType}", Pos);
                    SkipWhitespace();
                    if (HasMore && Peek == ',') { Pos++; SkipWhitespace(); }
                    else break;
                }
                SkipWhitespace();
                Expect(']');
                return new NbtLongArray(values.ToArray());
            }
        }

        private string ParseQuotedString()
        {
            var quote = _src[Pos];
            Pos++;
            var sb = new StringBuilder();
            while (Pos < _src.Length)
            {
                var c = _src[Pos++];
                if (c == quote) return sb.ToString();
                if (c == '\\')
                {
                    if (Pos >= _src.Length) throw new SnbtParseException("Unterminated escape", Pos);
                    var esc = _src[Pos++];
                    switch (esc)
                    {
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append('\''); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '0': sb.Append('\0'); break;
                        case 'u':
                            if (Pos + 4 > _src.Length)
                                throw new SnbtParseException("Truncated \\u escape", Pos);
                            var hex = _src.Substring(Pos, 4);
                            if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var u))
                                throw new SnbtParseException("Bad \\u escape", Pos);
                            sb.Append((char) u);
                            Pos += 4;
                            break;
                        default:
                            throw new SnbtParseException($"Unknown escape \\{esc}", Pos - 1);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            throw new SnbtParseException("Unterminated string literal", Pos);
        }

        private NbtTag ParseScalar()
        {
            var start = Pos;
            while (Pos < _src.Length && !IsValueDelimiter(_src[Pos])) Pos++;
            if (Pos == start) throw new SnbtParseException("Expected value", start);
            var token = _src.Substring(start, Pos - start);

            if (token == "true") return new NbtByte(1);
            if (token == "false") return new NbtByte(0);

            var last = token[token.Length - 1];
            var hasSuffix = char.IsLetter(last);
            var body = hasSuffix ? token.Substring(0, token.Length - 1) : token;

            if (hasSuffix)
            {
                switch (last)
                {
                    case 'b': case 'B':
                        if (sbyte.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sb1))
                            return new NbtByte((byte) sb1);
                        break;
                    case 's': case 'S':
                        if (short.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sh))
                            return new NbtShort(sh);
                        break;
                    case 'l': case 'L':
                        if (long.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lo))
                            return new NbtLong(lo);
                        break;
                    case 'f': case 'F':
                        if (float.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out var fl))
                            return new NbtFloat(fl);
                        break;
                    case 'd': case 'D':
                        if (double.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out var dl))
                            return new NbtDouble(dl);
                        break;
                }
            }
            else
            {
                if (token.IndexOf('.') >= 0 || token.IndexOf('e') >= 0 || token.IndexOf('E') >= 0)
                {
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                        return new NbtDouble(d);
                }
                else if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    return new NbtInt(i);
                }
                else if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                {
                    return new NbtLong(l);
                }
            }

            return new NbtString(token);
        }

        private static bool IsValueDelimiter(char c)
        {
            return c == ',' || c == '}' || c == ']' || c == ':' || char.IsWhiteSpace(c);
        }

        private void Expect(char c)
        {
            if (Pos >= _src.Length || _src[Pos] != c)
                throw new SnbtParseException($"Expected '{c}'", Pos);
            Pos++;
        }
    }
}
