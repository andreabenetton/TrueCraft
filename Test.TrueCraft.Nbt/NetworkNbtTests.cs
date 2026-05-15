using System.IO;
using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;
using Xunit;

namespace Test.TrueCraft.Nbt
{
    /// <summary>
    ///     Tests for the nameless-root NBT framing introduced in the Java Edition
    ///     1.20.2 protocol. On the wire the root TAG_Compound is preceded only by
    ///     its tag-id byte (0x0A) — no length-prefixed name.
    /// </summary>
    public class NetworkNbtTests
    {
        [Fact]
        public void DefaultRootHasNameIsTrue()
        {
            Assert.True(new NbtFile().RootHasName);
        }

        [Fact]
        public void Save_NamelessRoot_SkipsRootName()
        {
            var file = new NbtFile(new NbtCompound("root") { new NbtInt("v", 7) })
            {
                RootHasName = false,
                UseStandardUtf8 = true,
            };
            var buf = file.SaveToBuffer(NbtCompression.None);

            // Expect: 0x0A (compound) + body + 0x00 (end).
            // Disk framing would be 0x0A + 0x00 0x04 'r' 'o' 'o' 't' + body + 0x00.
            // For the network shape the second byte must already be the first child tag-id.
            Assert.Equal((byte) NbtTagType.Compound, buf[0]);
            Assert.Equal((byte) NbtTagType.Int, buf[1]); // first child tag-id, no root name
        }

        [Fact]
        public void Save_DiskRoot_HasRootName()
        {
            var file = new NbtFile(new NbtCompound("root") { new NbtInt("v", 7) });
            var buf = file.SaveToBuffer(NbtCompression.None);

            // Disk framing: 0x0A 0x00 0x04 'r' 'o' 'o' 't' ...
            Assert.Equal((byte) NbtTagType.Compound, buf[0]);
            Assert.Equal(0x00, buf[1]); // name length high
            Assert.Equal(0x04, buf[2]); // name length low
            Assert.Equal((byte) 'r', buf[3]);
            Assert.Equal((byte) 'o', buf[4]);
            Assert.Equal((byte) 'o', buf[5]);
            Assert.Equal((byte) 't', buf[6]);
            Assert.Equal((byte) NbtTagType.Int, buf[7]); // first child tag-id
        }

        [Fact]
        public void NamelessRoot_RoundTrip()
        {
            var original = new NbtFile(
                new NbtCompound("ignored-on-the-wire") { new NbtString("greeting", "hello"), new NbtInt("count", 42) })
            {
                RootHasName = false,
                UseStandardUtf8 = true,
            };
            var buf = original.SaveToBuffer(NbtCompression.None);

            var loaded = new NbtFile
            {
                RootHasName = false,
                UseStandardUtf8 = true,
            };
            loaded.LoadFromBuffer(buf, 0, buf.Length, NbtCompression.None);

            Assert.Equal(string.Empty, loaded.RootTag.Name);
            Assert.Equal("hello", loaded.RootTag["greeting"].StringValue);
            Assert.Equal(42, loaded.RootTag["count"].IntValue);
        }

        [Fact]
        public void NamelessRoot_LoadDiskFile_AsNetwork_MisinterpretsBytes()
        {
            // Sanity check: loading a disk-framed file with RootHasName=false will
            // misalign — the first child's tag-id byte will be read as the next byte
            // after the (unread) root name. This documents the negative case.
            var disk = new NbtFile(new NbtCompound("root") { new NbtInt("v", 7) });
            var diskBuf = disk.SaveToBuffer(NbtCompression.None);

            var loaded = new NbtFile { RootHasName = false };
            // The behaviour is undefined / format-error; just check we don't crash.
            try { loaded.LoadFromBuffer(diskBuf, 0, diskBuf.Length, NbtCompression.None); }
            catch (NbtFormatException) { /* expected */ }
            catch (EndOfStreamException) { /* also acceptable */ }
        }
    }
}
