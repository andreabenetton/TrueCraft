using TrueCraft.Nbt;
using TrueCraft.Nbt.Snbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt
{
    public class SnbtParserTests
    {
        // --- Scalars ---

        [Fact] public void Parse_Int()       => Assert.Equal(42,            ((NbtInt) SnbtParser.Parse("42")).Value);
        [Fact] public void Parse_NegInt()    => Assert.Equal(-7,            ((NbtInt) SnbtParser.Parse("-7")).Value);
        [Fact] public void Parse_Long()      => Assert.Equal(1234567890L,   ((NbtLong) SnbtParser.Parse("1234567890L")).Value);
        [Fact] public void Parse_LongLower() => Assert.Equal(1234L,          ((NbtLong) SnbtParser.Parse("1234l")).Value);
        [Fact] public void Parse_Byte()      => Assert.Equal(42,            ((NbtByte) SnbtParser.Parse("42b")).Value);
        [Fact] public void Parse_NegByte()   => Assert.Equal(-1,            (sbyte) ((NbtByte) SnbtParser.Parse("-1b")).Value);
        [Fact] public void Parse_Short()     => Assert.Equal((short)123,    ((NbtShort) SnbtParser.Parse("123s")).Value);
        [Fact] public void Parse_Float()     => Assert.Equal(1.5f,          ((NbtFloat) SnbtParser.Parse("1.5f")).Value);
        [Fact] public void Parse_Double()    => Assert.Equal(2.5,           ((NbtDouble) SnbtParser.Parse("2.5d")).Value);
        [Fact] public void Parse_DoubleNoSuffix() => Assert.Equal(3.14,     ((NbtDouble) SnbtParser.Parse("3.14")).Value);

        [Fact] public void Parse_TrueIsByte1()  => Assert.Equal(1, ((NbtByte) SnbtParser.Parse("true")).Value);
        [Fact] public void Parse_FalseIsByte0() => Assert.Equal(0, ((NbtByte) SnbtParser.Parse("false")).Value);

        // --- Strings ---

        [Fact] public void Parse_DoubleQuoted() => Assert.Equal("hello", ((NbtString) SnbtParser.Parse("\"hello\"")).Value);
        [Fact] public void Parse_SingleQuoted() => Assert.Equal("hi",    ((NbtString) SnbtParser.Parse("'hi'")).Value);
        [Fact] public void Parse_EscapedQuote() => Assert.Equal("a\"b",  ((NbtString) SnbtParser.Parse("\"a\\\"b\"")).Value);
        [Fact] public void Parse_EscapedBackslash() => Assert.Equal("a\\b", ((NbtString) SnbtParser.Parse("\"a\\\\b\"")).Value);
        [Fact] public void Parse_UnicodeEscape() => Assert.Equal("中", ((NbtString) SnbtParser.Parse("\"\\u4E2D\"")).Value);

        // --- Compound / list / arrays ---

        [Fact]
        public void Parse_EmptyCompound()
        {
            var t = SnbtParser.Parse("{}");
            Assert.IsType<NbtCompound>(t);
            Assert.Empty((NbtCompound) t);
        }

        [Fact]
        public void Parse_SimpleCompound()
        {
            var c = (NbtCompound) SnbtParser.Parse("{name:\"hello\",count:5b}");
            Assert.Equal("hello", c["name"].StringValue);
            Assert.Equal(5, c["count"].ByteValue);
        }

        [Fact]
        public void Parse_CompoundTrailingComma()
        {
            var c = (NbtCompound) SnbtParser.Parse("{a:1,b:2,}");
            Assert.Equal(1, c["a"].IntValue);
            Assert.Equal(2, c["b"].IntValue);
        }

        [Fact]
        public void Parse_EmptyList()
        {
            var l = (NbtList) SnbtParser.Parse("[]");
            Assert.Empty(l);
        }

        [Fact]
        public void Parse_HomogeneousList()
        {
            var l = (NbtList) SnbtParser.Parse("[1, 2, 3]");
            Assert.Equal(NbtTagType.Int, l.ListType);
            Assert.Equal(3, l.Count);
            Assert.Equal(1, l.Get<NbtInt>(0).Value);
            Assert.Equal(3, l.Get<NbtInt>(2).Value);
        }

        [Fact]
        public void Parse_HeterogeneousList_Fails()
        {
            Assert.Throws<SnbtParseException>(() => SnbtParser.Parse("[1, \"two\", 3]"));
        }

        [Fact]
        public void Parse_ByteArray()
        {
            var arr = (NbtByteArray) SnbtParser.Parse("[B; 1b, 2b, 3b]");
            Assert.Equal(new byte[] { 1, 2, 3 }, arr.Value);
        }

        [Fact]
        public void Parse_IntArray()
        {
            var arr = (NbtIntArray) SnbtParser.Parse("[I; 10, 20, 30]");
            Assert.Equal(new[] { 10, 20, 30 }, arr.Value);
        }

        [Fact]
        public void Parse_LongArray()
        {
            var arr = (NbtLongArray) SnbtParser.Parse("[L; 100L, 200L, 300L]");
            Assert.Equal(new[] { 100L, 200L, 300L }, arr.Value);
        }

        [Fact]
        public void Parse_NestedCompoundInList()
        {
            var c = (NbtCompound) SnbtParser.Parse("{items: [{id: \"sword\", count: 1b}, {id: \"axe\", count: 2b}]}");
            var list = c.Get<NbtList>("items");
            Assert.Equal(2, list.Count);
            Assert.Equal("sword", list.Get<NbtCompound>(0)["id"].StringValue);
            Assert.Equal(2, list.Get<NbtCompound>(1)["count"].ByteValue);
        }

        [Fact]
        public void Parse_QuotedKey()
        {
            var c = (NbtCompound) SnbtParser.Parse("{\"with spaces\": 1, normal: 2}");
            Assert.Equal(1, c["with spaces"].IntValue);
            Assert.Equal(2, c["normal"].IntValue);
        }

        [Fact]
        public void Parse_MalformedMissingClose_Fails()
        {
            Assert.Throws<SnbtParseException>(() => SnbtParser.Parse("{a:1"));
        }

        [Fact]
        public void Parse_MalformedTrailingInput_Fails()
        {
            Assert.Throws<SnbtParseException>(() => SnbtParser.Parse("{a:1} extra"));
        }

        [Fact]
        public void Parse_NumericSuffixes_AllVariants()
        {
            Assert.IsType<NbtByte>(SnbtParser.Parse("1b"));
            Assert.IsType<NbtByte>(SnbtParser.Parse("1B"));
            Assert.IsType<NbtShort>(SnbtParser.Parse("1s"));
            Assert.IsType<NbtShort>(SnbtParser.Parse("1S"));
            Assert.IsType<NbtLong>(SnbtParser.Parse("1l"));
            Assert.IsType<NbtLong>(SnbtParser.Parse("1L"));
            Assert.IsType<NbtFloat>(SnbtParser.Parse("1.0f"));
            Assert.IsType<NbtFloat>(SnbtParser.Parse("1.0F"));
            Assert.IsType<NbtDouble>(SnbtParser.Parse("1.0d"));
            Assert.IsType<NbtDouble>(SnbtParser.Parse("1.0D"));
        }
    }
}
