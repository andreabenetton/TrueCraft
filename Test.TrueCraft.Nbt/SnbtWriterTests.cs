using TrueCraft.Nbt;
using TrueCraft.Nbt.Snbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt
{
    public class SnbtWriterTests
    {
        [Fact] public void Write_Int()    => Assert.Equal("42",    SnbtWriter.ToSnbt(new NbtInt(42)));
        [Fact] public void Write_Long()   => Assert.Equal("42L",   SnbtWriter.ToSnbt(new NbtLong(42)));
        [Fact] public void Write_Byte()   => Assert.Equal("5b",    SnbtWriter.ToSnbt(new NbtByte(5)));
        [Fact] public void Write_Short()  => Assert.Equal("7s",    SnbtWriter.ToSnbt(new NbtShort(7)));
        [Fact] public void Write_Float()  => Assert.Equal("1.5f",  SnbtWriter.ToSnbt(new NbtFloat(1.5f)));
        [Fact] public void Write_Double() => Assert.Equal("2.5d",  SnbtWriter.ToSnbt(new NbtDouble(2.5)));
        [Fact] public void Write_String() => Assert.Equal("\"hi\"", SnbtWriter.ToSnbt(new NbtString("hi")));

        [Fact]
        public void Write_StringWithDoubleQuote_UsesSingleQuote()
        {
            Assert.Equal("'he said \"hi\"'", SnbtWriter.ToSnbt(new NbtString("he said \"hi\"")));
        }

        [Fact]
        public void Write_StringWithBothQuotes_UsesDoubleAndEscapes()
        {
            Assert.Equal("\"\\\"and'\"", SnbtWriter.ToSnbt(new NbtString("\"and'")));
        }

        [Fact]
        public void Write_EmptyCompound() => Assert.Equal("{}", SnbtWriter.ToSnbt(new NbtCompound()));
        [Fact] public void Write_EmptyList() => Assert.Equal("[]", SnbtWriter.ToSnbt(new NbtList()));

        [Fact]
        public void Write_SimpleCompound()
        {
            var c = new NbtCompound { new NbtString("name", "hello"), new NbtByte("count", 5) };
            Assert.Equal("{name:\"hello\",count:5b}", SnbtWriter.ToSnbt(c));
        }

        [Fact]
        public void Write_QuotedKey_WhenNotIdentifier()
        {
            var c = new NbtCompound { new NbtInt("with spaces", 1) };
            Assert.Equal("{\"with spaces\":1}", SnbtWriter.ToSnbt(c));
        }

        [Fact]
        public void Write_IntArray() => Assert.Equal("[I;1,2,3]", SnbtWriter.ToSnbt(new NbtIntArray(new[] { 1, 2, 3 })));
        [Fact] public void Write_ByteArray() => Assert.Equal("[B;1b,-1b,127b]",
            SnbtWriter.ToSnbt(new NbtByteArray(new byte[] { 1, 0xFF, 0x7F })));
        [Fact] public void Write_LongArray() => Assert.Equal("[L;1L,2L]", SnbtWriter.ToSnbt(new NbtLongArray(new[] { 1L, 2L })));

        [Fact]
        public void Write_ListOfInts()
        {
            var list = new NbtList(NbtTagType.Int) { new NbtInt(1), new NbtInt(2), new NbtInt(3) };
            Assert.Equal("[1,2,3]", SnbtWriter.ToSnbt(list));
        }

        [Fact]
        public void PrettyPrint_NestedCompound_IsIndented()
        {
            var c = new NbtCompound { new NbtCompound("inner") { new NbtInt("x", 1) } };
            var s = SnbtWriter.ToSnbt(c, pretty: true);
            Assert.Contains("\n  inner:", s);
            Assert.Contains("\n    x:", s);
        }
    }
}
