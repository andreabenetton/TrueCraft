using System.IO;
using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt
{
    /// <summary>
    ///     Tests for NbtFile.MaxDepth — the configurable cap on nested compound/list
    ///     depth that protects against stack-overflow on adversarial input.
    /// </summary>
    public class DepthLimitTests
    {
        [Fact]
        public void DefaultMaxDepthIs512()
        {
            Assert.Equal(512, new NbtFile().MaxDepth);
        }

        [Fact]
        public void RealisticNesting_IsAccepted()
        {
            // 8 nested compounds — well within any reasonable cap.
            var deepest = new NbtCompound("d8") { new NbtInt("leaf", 42) };
            for (var i = 7; i >= 1; i--)
                deepest = new NbtCompound("d" + i) { deepest };
            var root = new NbtCompound("root") { deepest };

            var file = new NbtFile(root);
            var buf = file.SaveToBuffer(NbtCompression.None);
            var loaded = new NbtFile();
            loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
            Assert.Equal(42, loaded.RootTag["d1"]["d2"]["d3"]["d4"]["d5"]["d6"]["d7"]["d8"]["leaf"].IntValue);
        }

        [Fact]
        public void DepthBomb_AtDefaultLimit_IsRejected()
        {
            // Hand-craft a stream with 1000 nested compounds (no TAG_End closures).
            // The 513th open-compound must trip the MaxDepth=512 guard.
            using var ms = new MemoryStream();
            for (var i = 0; i < 1000; i++)
            {
                ms.WriteByte((byte) NbtTagType.Compound);
                ms.WriteByte(0x00); // length high
                ms.WriteByte(0x01); // length low
                ms.WriteByte((byte) 'n'); // name
            }
            ms.Position = 0;

            var bytes = ms.ToArray();
            var loaded = new NbtFile();
            Assert.Throws<NbtFormatException>(() =>
                loaded.LoadFromBuffer(bytes, 0, bytes.Length, NbtCompression.None));
        }

        [Fact]
        public void DepthBomb_RaisedLimit_IsAccepted_UpToCount()
        {
            // Build a well-formed file with 100 nested compounds.
            var current = new NbtCompound("d100");
            for (var i = 99; i >= 1; i--)
                current = new NbtCompound("d" + i) { current };
            var root = new NbtCompound("root") { current };
            var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);

            var loaded = new NbtFile { MaxDepth = 200 };
            loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
            Assert.NotNull(loaded.RootTag);
        }

        [Fact]
        public void DepthBomb_AtLoweredLimit_IsRejected()
        {
            // 12-deep nesting, MaxDepth=5 — must trip.
            var current = new NbtCompound("d12");
            for (var i = 11; i >= 1; i--)
                current = new NbtCompound("d" + i) { current };
            var root = new NbtCompound("root") { current };
            var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);

            var loaded = new NbtFile { MaxDepth = 5 };
            Assert.Throws<NbtFormatException>(() =>
                loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None));
        }

        [Fact]
        public void DepthBomb_DisabledLimit_AllowsDeepNesting()
        {
            var current = new NbtCompound("d20");
            for (var i = 19; i >= 1; i--)
                current = new NbtCompound("d" + i) { current };
            var root = new NbtCompound("root") { current };
            var buf = new NbtFile(root).SaveToBuffer(NbtCompression.None);

            var loaded = new NbtFile { MaxDepth = 0 };
            loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);
            Assert.NotNull(loaded.RootTag);
        }
    }
}
