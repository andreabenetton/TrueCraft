using System;
using System.IO;
using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt;

/// <summary>
///     Coverage for TAG_Long_Array (id 0x0C, added in Minecraft 1.12).
/// </summary>
public class LongArrayTests
{
    [Fact]
    public void TagTypeAndEnumValue()
    {
        Assert.Equal((byte) 0x0C, (byte) NbtTagType.LongArray);
        Assert.Equal(NbtTagType.LongArray, new NbtLongArray().TagType);
        Assert.Equal("TAG_Long_Array", NbtTag.GetCanonicalTagName(NbtTagType.LongArray));
    }

    [Fact]
    public void EmptyArrayDefaults()
    {
        var tag = new NbtLongArray("empty");
        Assert.Equal("empty", tag.Name);
        Assert.Empty(tag.Value);
    }

    [Fact]
    public void ValueIsClonedOnConstruction()
    {
        var src = new long[] { 1, 2, 3 };
        var tag = new NbtLongArray("data", src);
        src[0] = 999;
        Assert.Equal(1L, tag[0]); // tag's copy is independent
    }

    [Fact]
    public void IndexerGetSet()
    {
        var tag = new NbtLongArray("data", new long[] { 10L, 20L, 30L });
        Assert.Equal(20L, tag[1]);
        tag[1] = -7L;
        Assert.Equal(-7L, tag[1]);
    }

    [Fact]
    public void DeepCopyClonesArray()
    {
        var original = new NbtLongArray("a", new long[] { 7L, 8L });
        var copy = (NbtLongArray) original.Clone();
        copy[0] = 99L;
        Assert.Equal(7L, original[0]); // original untouched
    }

    [Fact]
    public void RoundTripUncompressed()
    {
        var root = new NbtCompound("root") { new NbtLongArray("data", new[] { -1L, 0L, long.MaxValue, long.MinValue }) };
        var file = new NbtFile(root);
        var buf = file.SaveToBuffer(NbtCompression.None);

        var loaded = new NbtFile();
        loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);

        var arr = loaded.RootTag.Get<NbtLongArray>("data");
        Assert.NotNull(arr);
        Assert.Equal(new[] { -1L, 0L, long.MaxValue, long.MinValue }, arr.Value);
    }

    [Fact]
    public void RoundTripGZipAndZLib()
    {
        var root = new NbtCompound("root") { new NbtLongArray("data", new[] { 42L, 1337L }) };
        var file = new NbtFile(root);

        foreach (var compression in new[] { NbtCompression.GZip, NbtCompression.ZLib })
        {
            var buf = file.SaveToBuffer(compression);
            var loaded = new NbtFile();
            loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.AutoDetect);
            Assert.Equal(new[] { 42L, 1337L }, loaded.RootTag.Get<NbtLongArray>("data").Value);
        }
    }

    [Fact]
    public void OnTheWireLayoutIsBigEndian()
    {
        // Hand-build a minimal file: { root: { data: long[] { 0x0102030405060708 } } }
        // Layout:
        //   0x0a 'r' 'o' 'o' 't'                  (root tag-id + len-prefixed name)
        //     0x0c                                (LongArray tag id)
        //     'd' 'a' 't' 'a'                     (name "data")
        //     0x00 0x00 0x00 0x01                 (length = 1)
        //     0x01 0x02 0x03 0x04 0x05 0x06 0x07 0x08  (value, big-endian)
        //   0x00                                  (TAG_End closing root)
        var tag = new NbtLongArray("data", new[] { 0x0102030405060708L });
        var root = new NbtCompound("root") { tag };
        var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);

        // The payload bytes 0x01..0x08 must appear in big-endian order.
        int idx = -1;
        for (var i = 0; i < buf.Length - 7; i++)
        {
            if (buf[i] == 0x01 && buf[i + 1] == 0x02 && buf[i + 2] == 0x03 &&
                buf[i + 3] == 0x04 && buf[i + 4] == 0x05 && buf[i + 5] == 0x06 &&
                buf[i + 6] == 0x07 && buf[i + 7] == 0x08)
            {
                idx = i;
                break;
            }
        }

        Assert.True(idx > 0, "Big-endian long not found in saved buffer");
    }

    [Fact]
    public void NegativeLengthRejected()
    {
        // Build bytes that fake a TAG_Long_Array with length = -1.
        // root: { data: <negative-length long array> }
        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write((byte) NbtTagType.Compound);
            WriteUtfString(bw, "root");
            bw.Write((byte) NbtTagType.LongArray);
            WriteUtfString(bw, "data");
            // length, big-endian
            bw.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            bw.Write((byte) NbtTagType.End);
        }

        var bytes = ms.ToArray();
        var loaded = new NbtFile();
        Assert.Throws<NbtFormatException>(() =>
            loaded.LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
    }

    [Fact]
    public void NestedInsideListInsideCompound()
    {
        var inner = new NbtList("longArrays", NbtTagType.LongArray)
        {
            new NbtLongArray(new long[] { 1L, 2L, 3L }),
            new NbtLongArray(new long[] { 4L, 5L, 6L }),
        };
        var root = new NbtCompound("root") { inner };
        var file = new NbtFile(root);

        var buf = file.SaveToBuffer(NbtCompression.None);
        var loaded = new NbtFile();
        loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);

        var list = loaded.RootTag.Get<NbtList>("longArrays");
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { 1L, 2L, 3L }, list.Get<NbtLongArray>(0).Value);
        Assert.Equal(new[] { 4L, 5L, 6L }, list.Get<NbtLongArray>(1).Value);
    }

    // Write a length-prefixed Java-style modified UTF-8 string. For ASCII the
    // encoding is identical to standard UTF-8 so we can hand-roll it.
    private static void WriteUtfString(BinaryWriter bw, string s)
    {
        ushort length = (ushort) s.Length;
        bw.Write((byte) (length >> 8));
        bw.Write((byte) (length & 0xFF));
        foreach (var c in s) bw.Write((byte) c);
    }
}
