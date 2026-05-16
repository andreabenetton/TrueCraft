using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt;

/// <summary>
///     Tests for the JavaModifiedUtf8 codec and its integration with NbtFile.
///
///     Modified UTF-8 differs from standard UTF-8 in exactly two places:
///     U+0000 is encoded as the overlong sequence 0xC0 0x80, and supplementary-plane
///     chars (U+10000..U+10FFFF) are encoded as two 3-byte CESU-8 surrogate halves,
///     not a single 4-byte UTF-8 sequence. These tests pin both behaviours bit-for-bit.
/// </summary>
public class ModifiedUtf8Tests
{
    // --- Direct codec tests ---

    [Fact]
    public void Empty_RoundTrip()
    {
        var bytes = JavaModifiedUtf8.Encode("");
        Assert.Empty(bytes);
        Assert.Equal("", JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void Ascii_RoundTrip_BitForBit()
    {
        const string s = "Hello, world!";
        var bytes = JavaModifiedUtf8.Encode(s);
        Assert.Equal(new byte[]
        {
            0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x21
        }, bytes);
        Assert.Equal(s, JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void Null_EncodesAsOverlongC080()
    {
        var bytes = JavaModifiedUtf8.Encode("\0");
        Assert.Equal(new byte[] { 0xC0, 0x80 }, bytes);
        Assert.Equal("\0", JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void NullInTheMiddle_RoundTrip()
    {
        const string s = "ab\0cd";
        var bytes = JavaModifiedUtf8.Encode(s);
        Assert.Equal(new byte[] { 0x61, 0x62, 0xC0, 0x80, 0x63, 0x64 }, bytes);
        Assert.Equal(s, JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void LatinSupplementary_RoundTrip()
    {
        // U+00E9 (é) -> standard UTF-8 / MUTF-8 both: 0xC3 0xA9.
        var bytes = JavaModifiedUtf8.Encode("café");
        Assert.Equal(new byte[] { 0x63, 0x61, 0x66, 0xC3, 0xA9 }, bytes);
        Assert.Equal("café", JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void CJK_RoundTrip()
    {
        // U+4E2D (中) -> 0xE4 0xB8 0xAD. Standard UTF-8 and MUTF-8 agree here.
        var bytes = JavaModifiedUtf8.Encode("中");
        Assert.Equal(new byte[] { 0xE4, 0xB8, 0xAD }, bytes);
        Assert.Equal("中", JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void SupplementaryEmoji_EncodesAsCESU8_SixBytes()
    {
        // U+1F9F1 (brick) is in the supplementary plane.
        // .NET stores it as a UTF-16 surrogate pair: 0xD83E 0xDDF1.
        // MUTF-8 emits two 3-byte CESU-8 sequences (six bytes total),
        // NOT the single 4-byte standard-UTF-8 form (F0 9F A7 B1).
        const string brick = "🧱";
        var bytes = JavaModifiedUtf8.Encode(brick);
        Assert.Equal(6, bytes.Length);
        Assert.Equal(new byte[] { 0xED, 0xA0, 0xBE, 0xED, 0xB7, 0xB1 }, bytes);
        Assert.Equal(brick, JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void StandardUtf8_FourByteForm_IsRejected()
    {
        // The 4-byte standard-UTF-8 encoding of the brick char (0xF0 0x9F 0xA7 0xB1)
        // is NOT valid Modified UTF-8 - Mojang's parser would reject it; ours must too.
        var bytes = new byte[] { 0xF0, 0x9F, 0xA7, 0xB1 };
        Assert.Throws<NbtFormatException>(() => JavaModifiedUtf8.Decode(bytes));
    }

    [Fact]
    public void TruncatedSequence_IsRejected()
    {
        // 0xC3 starts a 2-byte sequence but no continuation byte follows.
        Assert.Throws<NbtFormatException>(() => JavaModifiedUtf8.Decode(new byte[] { 0xC3 }));
    }

    [Fact]
    public void BadContinuationByte_IsRejected()
    {
        // 0xC3 followed by 0x20 (which is not 10xxxxxx) is invalid.
        Assert.Throws<NbtFormatException>(() => JavaModifiedUtf8.Decode(new byte[] { 0xC3, 0x20 }));
    }

    // --- Integration tests through NbtFile ---

    [Fact]
    public void NbtFile_EmbeddedNull_RoundTrips()
    {
        var root = new NbtCompound("root") { new NbtString("name", "with\0null") };
        var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);
        var loaded = new NbtFile();
        loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
        Assert.Equal("with\0null", loaded.RootTag["name"].StringValue);
    }

    [Fact]
    public void NbtFile_Emoji_RoundTrips()
    {
        const string s = "block 🧱 sound 🔊";
        var root = new NbtCompound("root") { new NbtString("desc", s) };
        var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);
        var loaded = new NbtFile();
        loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
        Assert.Equal(s, loaded.RootTag["desc"].StringValue);
    }

    [Fact]
    public void NbtFile_BigTest_RoundTripBytesAreStable()
    {
        // bigtest.nbt is the canonical fNbt test fixture. After loading through the
        // Modified UTF-8 path and saving back, the in-memory bytes must re-load
        // identically (the strings inside don't contain NUL or supplementary chars,
        // so byte-for-byte equivalence is expected).
        var file = new NbtFile();
        file.LoadFromFile(TestFiles.Big);
        var saved = file.SaveToBuffer(NbtCompression.None);

        var reloaded = new NbtFile();
        reloaded.LoadFromBuffer(saved, 0, saved.Length, NbtCompression.None);
        TestFiles.AssertNbtBigFile(reloaded);
    }

    [Fact]
    public void StandardUtf8_Flag_UsesUtf8NotMutf8()
    {
        // When UseStandardUtf8 is set, NUL must encode as a single 0x00 byte and
        // supplementary chars as the standard 4-byte UTF-8 form.
        var file = new NbtFile(new NbtCompound("root") { new NbtString("v", "a\0b") })
        {
            UseStandardUtf8 = true,
        };
        var buf = file.SaveToBuffer(NbtCompression.None);
        int idx = -1;
        for (var i = 0; i < buf.Length - 2; i++)
            if (buf[i] == 0x61 && buf[i + 1] == 0x00 && buf[i + 2] == 0x62)
            {
                idx = i;
                break;
            }
        Assert.True(idx >= 0, "Standard UTF-8 'a\\0b' not found in saved buffer.");
    }
}
