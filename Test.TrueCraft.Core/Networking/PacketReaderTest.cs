using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Core.Networking
{
    public class PacketReaderTest
    {
        // Pins the higher-level PacketReader.ReadPackets() framing semantics: the per-client
        // PacketBuffer survives across calls, packets get yielded in arrival order, and a packet
        // that spans two ReadPackets() invocations is reassembled correctly.

        private static (PacketReader reader, byte[] hello, byte[] world, byte[] keepAlive) Setup()
        {
            var reader = new PacketReader();
            reader.RegisterCorePackets();
            return (reader,
                Serialize(reader, new ChatMessagePacket("hello")),
                Serialize(reader, new ChatMessagePacket("world")),
                Serialize(reader, new KeepAlivePacket()));
        }

        private static byte[] Serialize<T>(PacketReader reader, T packet) where T : IPacket
        {
            using var ms = new MemoryStream();
            using var stream = new MinecraftStream(ms);
            reader.WritePacket(stream, packet);
            return ms.ToArray();
        }

        [Fact]
        public void SinglePacket_OneCall_YieldsOne()
        {
            var (reader, hello, _, _) = Setup();
            var key = new object();

            var packets = reader.ReadPackets(key, hello, 0, hello.Length).ToList();

            Assert.Single(packets);
            Assert.Equal("hello", ((ChatMessagePacket)packets[0]).Message);
        }

        [Fact]
        public void TwoPackets_OneCall_YieldsBothInOrder()
        {
            var (reader, hello, world, _) = Setup();
            var key = new object();

            var combined = new byte[hello.Length + world.Length];
            System.Buffer.BlockCopy(hello, 0, combined, 0, hello.Length);
            System.Buffer.BlockCopy(world, 0, combined, hello.Length, world.Length);

            var packets = reader.ReadPackets(key, combined, 0, combined.Length).ToList();

            Assert.Equal(2, packets.Count);
            Assert.Equal("hello", ((ChatMessagePacket)packets[0]).Message);
            Assert.Equal("world", ((ChatMessagePacket)packets[1]).Message);
        }

        [Fact]
        public void SplitPacket_TwoCalls_YieldsOnSecond()
        {
            var (reader, hello, _, _) = Setup();
            var key = new object();

            var half = hello.Length / 2;
            var first = reader.ReadPackets(key, hello, 0, half).ToList();
            Assert.Empty(first); // not enough bytes yet

            var second = reader.ReadPackets(key, hello, half, hello.Length - half).ToList();
            Assert.Single(second);
            Assert.Equal("hello", ((ChatMessagePacket)second[0]).Message);
        }

        [Fact]
        public void TwoPackets_SplitAcrossSecond_YieldsAcrossCalls()
        {
            var (reader, hello, world, _) = Setup();
            var key = new object();

            // First call carries hello + first byte of world.
            var seg1 = new byte[hello.Length + 1];
            System.Buffer.BlockCopy(hello, 0, seg1, 0, hello.Length);
            seg1[hello.Length] = world[0];

            var first = reader.ReadPackets(key, seg1, 0, seg1.Length).ToList();
            Assert.Single(first);
            Assert.Equal("hello", ((ChatMessagePacket)first[0]).Message);

            // Second call carries the rest of world.
            var second = reader.ReadPackets(key, world, 1, world.Length - 1).ToList();
            Assert.Single(second);
            Assert.Equal("world", ((ChatMessagePacket)second[0]).Message);
        }

        [Fact]
        public void PerKeyState_DoesNotLeakBetweenClients()
        {
            // Two clients (two keys) feed partial packets; their reassembly buffers must be independent.
            var (reader, hello, world, _) = Setup();
            var keyA = new object();
            var keyB = new object();

            var halfHello = hello.Length / 2;
            var halfWorld = world.Length / 2;

            // Each client sends half its packet first.
            Assert.Empty(reader.ReadPackets(keyA, hello, 0, halfHello).ToList());
            Assert.Empty(reader.ReadPackets(keyB, world, 0, halfWorld).ToList());

            // Now each sends the rest.
            var aDone = reader.ReadPackets(keyA, hello, halfHello, hello.Length - halfHello).ToList();
            var bDone = reader.ReadPackets(keyB, world, halfWorld, world.Length - halfWorld).ToList();

            Assert.Equal("hello", ((ChatMessagePacket)aDone.Single()).Message);
            Assert.Equal("world", ((ChatMessagePacket)bDone.Single()).Message);
        }

        [Fact]
        public void MixedPackets_KeepAliveThenChat_BothYielded()
        {
            var (reader, hello, _, keepAlive) = Setup();
            var key = new object();

            var combined = new byte[keepAlive.Length + hello.Length];
            System.Buffer.BlockCopy(keepAlive, 0, combined, 0, keepAlive.Length);
            System.Buffer.BlockCopy(hello, 0, combined, keepAlive.Length, hello.Length);

            var packets = reader.ReadPackets(key, combined, 0, combined.Length).ToList();

            Assert.Equal(2, packets.Count);
            Assert.IsType<KeepAlivePacket>(packets[0]);
            Assert.Equal("hello", ((ChatMessagePacket)packets[1]).Message);
        }
    }
}
