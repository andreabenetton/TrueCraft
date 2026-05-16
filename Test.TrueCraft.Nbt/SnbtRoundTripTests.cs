using TrueCraft.Nbt;
using TrueCraft.Nbt.Snbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt;

/// <summary>
///     For a representative set of NbtTag trees, assert that
///     <c>SnbtParser.Parse(SnbtWriter.ToSnbt(tag))</c> reproduces the original.
///     This is the forcing function pinning parser/writer parity.
/// </summary>
public class SnbtRoundTripTests
{
    public static TheoryData<string, NbtTag> Cases => new()
    {
        { "byte", new NbtByte(42) },
        { "short", new NbtShort(1234) },
        { "int", new NbtInt(int.MaxValue) },
        { "long", new NbtLong(long.MinValue) },
        { "float", new NbtFloat(1.5f) },
        { "double", new NbtDouble(2.5) },
        { "string", new NbtString("hello world") },
        { "string-empty", new NbtString("") },
        { "string-with-quote", new NbtString("she said \"hi\"") },
        { "byte-array", new NbtByteArray(new byte[] { 1, 2, 3, 4 }) },
        { "int-array", new NbtIntArray(new[] { 1, -2, 3, int.MaxValue }) },
        { "long-array", new NbtLongArray(new[] { 1L, -2L, long.MaxValue }) },
        { "empty-compound", new NbtCompound() },
        { "empty-list", new NbtList() },
        { "simple-compound", new NbtCompound
            {
                new NbtString("name", "alice"),
                new NbtInt("age", 30),
                new NbtByte("admin", 1),
            } },
        { "list-of-ints", new NbtList(NbtTagType.Int)
            {
                new NbtInt(1), new NbtInt(2), new NbtInt(3),
            } },
        { "nested", new NbtCompound
            {
                new NbtCompound("inner") { new NbtInt("x", 1), new NbtInt("y", 2) },
                new NbtList("items", NbtTagType.Compound)
                {
                    new NbtCompound { new NbtString("id", "sword"), new NbtByte("count", 1) },
                    new NbtCompound { new NbtString("id", "axe"),   new NbtByte("count", 2) },
                },
            } },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void RoundTrip(string label, NbtTag original)
    {
        _ = label;
        var snbt = SnbtWriter.ToSnbt(original);
        var parsed = SnbtParser.Parse(snbt);
        // Re-serialise and compare strings — equivalence via canonical SNBT form.
        Assert.Equal(snbt, SnbtWriter.ToSnbt(parsed));
    }
}
