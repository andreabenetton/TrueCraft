using System.IO;
using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt;

/// <summary>
///     Tests for NbtFile.MaxArrayElements — the configurable cap that protects
///     against adversarial NBT declaring a multi-gigabyte array length.
/// </summary>
public class AllocationGuardTests
{
    [Fact]
    public void DefaultMaxIs16Mi()
    {
        Assert.Equal(16 * 1024 * 1024, new NbtFile().MaxArrayElements);
    }

    [Fact]
    public void NegativeByteArrayLength_IsRejected()
    {
        var bytes = BuildArrayHeader(NbtTagType.ByteArray, length: -1);
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void NegativeIntArrayLength_IsRejected()
    {
        var bytes = BuildArrayHeader(NbtTagType.IntArray, length: -1);
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void NegativeLongArrayLength_IsRejected()
    {
        var bytes = BuildArrayHeader(NbtTagType.LongArray, length: -1);
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void NegativeListLength_IsRejected()
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte) NbtTagType.Compound);
        WriteName(ms, "root");
        ms.WriteByte((byte) NbtTagType.List);
        WriteName(ms, "list");
        ms.WriteByte((byte) NbtTagType.Byte);
        // length = -1, big-endian
        ms.WriteByte(0xFF); ms.WriteByte(0xFF); ms.WriteByte(0xFF); ms.WriteByte(0xFF);
        ms.WriteByte((byte) NbtTagType.End);
        var bytes = ms.ToArray();
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void EnormousIntArrayLength_IsRejected_NotOOM()
    {
        // length = int.MaxValue would normally OOM with a 8 GiB allocation. The
        // MaxArrayElements guard must trip first.
        var bytes = BuildArrayHeader(NbtTagType.IntArray, length: int.MaxValue);
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void EnormousLongArrayLength_IsRejected_NotOOM()
    {
        var bytes = BuildArrayHeader(NbtTagType.LongArray, length: int.MaxValue);
        Assert.Throws<NbtFormatException>(() =>
            new NbtFile().LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void LengthJustOverConfiguredMax_IsRejected()
    {
        var bytes = BuildArrayHeader(NbtTagType.IntArray, length: 1001);
        var file = new NbtFile { MaxArrayElements = 1000 };
        Assert.Throws<NbtFormatException>(() =>
            file.LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void DisabledLimit_AcceptsSmallArrays()
    {
        // Real ~1000-int array, no cap; must load.
        var data = new int[1000];
        for (var i = 0; i < data.Length; i++) data[i] = i;
        var root = new NbtCompound("root") { new NbtIntArray("data", data) };
        var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);

        var loaded = new NbtFile { MaxArrayElements = 0 };
        loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
        Assert.Equal(data, loaded.RootTag.Get<NbtIntArray>("data").Value);
    }

    // Build a minimal NBT stream of the form:
    //   { root: { data: <array-tag of given type with given declared length> } }
    // The payload bytes after the length are NOT written, so a real read of the
    // array body would hit end-of-stream — but the length guard fires first.
    private static byte[] BuildArrayHeader(NbtTagType arrayType, int length)
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte) NbtTagType.Compound);
        WriteName(ms, "root");
        ms.WriteByte((byte) arrayType);
        WriteName(ms, "data");
        // big-endian int32 length
        ms.WriteByte((byte) (length >> 24));
        ms.WriteByte((byte) (length >> 16));
        ms.WriteByte((byte) (length >> 8));
        ms.WriteByte((byte) length);
        ms.WriteByte((byte) NbtTagType.End);
        return ms.ToArray();
    }

    private static void WriteName(Stream s, string name)
    {
        // length-prefixed (unsigned big-endian 16-bit) ASCII name
        s.WriteByte((byte) (name.Length >> 8));
        s.WriteByte((byte) (name.Length & 0xFF));
        foreach (var c in name) s.WriteByte((byte) c);
    }
}
