using System.IO;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Core.Networking
{
    public class PacketSegmentProcessorTest
    {
        // Pins the current behavior of the segment processor before we replace the receive pump
        // with System.IO.Pipelines (Part 2, Phase N2). Each case feeds a different sequence of
        // (offset, length) slices and asserts the processor emits the same packets it would emit
        // if the whole buffer had arrived in one piece.

        private static (PacketReader reader, byte[] keepAliveBytes, byte[] chatHelloBytes, byte[] chatWorldBytes) Setup()
        {
            var reader = new PacketReader();
            reader.RegisterCorePackets();

            return (reader, Serialize(reader, new KeepAlivePacket()),
                Serialize(reader, new ChatMessagePacket("hello")),
                Serialize(reader, new ChatMessagePacket("world")));
        }

        private static byte[] Serialize<T>(PacketReader reader, T packet) where T : global::TrueCraft.API.Networking.IPacket
        {
            using var ms = new MemoryStream();
            using var stream = new MinecraftStream(ms);
            reader.WritePacket(stream, packet);
            return ms.ToArray();
        }

        [Fact]
        public void EmptyBuffer_YieldsNothing()
        {
            var (reader, _, _, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            var more = processor.ProcessNextSegment(new byte[0], 0, 0, out var packet);

            Assert.False(more);
            Assert.Null(packet);
        }

        [Fact]
        public void SinglePacket_WholeBuffer_YieldsOnePacket()
        {
            var (reader, keepAlive, _, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            var more = processor.ProcessNextSegment(keepAlive, 0, keepAlive.Length, out var packet);

            Assert.NotNull(packet);
            Assert.IsType<KeepAlivePacket>(packet);
            Assert.False(more); // KeepAlive is 1 byte; buffer drained
        }

        [Fact]
        public void SingleChatPacket_WholeBuffer_RoundTripsMessage()
        {
            var (reader, _, hello, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            processor.ProcessNextSegment(hello, 0, hello.Length, out var packet);

            var chat = Assert.IsType<ChatMessagePacket>(packet);
            Assert.Equal("hello", chat.Message);
        }

        [Fact]
        public void SinglePacket_SplitInHalf_RequiresBothSegments()
        {
            var (reader, _, hello, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            // First half: not enough to decode the packet.
            var half = hello.Length / 2;
            var more = processor.ProcessNextSegment(hello, 0, half, out var packet);
            Assert.Null(packet);
            Assert.False(more); // false: no packet yet AND nothing left to retry

            // Second half: completes the packet.
            more = processor.ProcessNextSegment(hello, half, hello.Length - half, out packet);
            var chat = Assert.IsType<ChatMessagePacket>(packet);
            Assert.Equal("hello", chat.Message);
            Assert.False(more); // exactly one packet, buffer empty after parse
        }

        [Fact]
        public void TwoPackets_OneSegment_YieldsBoth()
        {
            var (reader, _, hello, world) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            var combined = new byte[hello.Length + world.Length];
            System.Buffer.BlockCopy(hello, 0, combined, 0, hello.Length);
            System.Buffer.BlockCopy(world, 0, combined, hello.Length, world.Length);

            var more = processor.ProcessNextSegment(combined, 0, combined.Length, out var first);
            Assert.True(more); // more bytes remain after first packet
            Assert.Equal("hello", Assert.IsType<ChatMessagePacket>(first).Message);

            // Drain by feeding empty segments (mirrors PacketReader.ReadPackets' loop).
            more = processor.ProcessNextSegment(new byte[0], 0, 0, out var second);
            Assert.Equal("world", Assert.IsType<ChatMessagePacket>(second).Message);
            Assert.False(more);
        }

        [Fact]
        public void TwoPackets_SplitAcrossSecond_YieldsBoth()
        {
            var (reader, _, hello, world) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            // First segment: hello + first byte of world. Second segment: rest of world.
            var seg1 = new byte[hello.Length + 1];
            System.Buffer.BlockCopy(hello, 0, seg1, 0, hello.Length);
            seg1[hello.Length] = world[0];

            var seg2 = new byte[world.Length - 1];
            System.Buffer.BlockCopy(world, 1, seg2, 0, world.Length - 1);

            var more = processor.ProcessNextSegment(seg1, 0, seg1.Length, out var first);
            Assert.Equal("hello", Assert.IsType<ChatMessagePacket>(first).Message);
            Assert.True(more); // 1 byte of `world` still buffered, but not enough to parse

            // Drain attempt: still incomplete.
            more = processor.ProcessNextSegment(new byte[0], 0, 0, out var stillNothing);
            Assert.Null(stillNothing);
            Assert.False(more);

            // Provide the rest of the second packet.
            more = processor.ProcessNextSegment(seg2, 0, seg2.Length, out var second);
            Assert.Equal("world", Assert.IsType<ChatMessagePacket>(second).Message);
            Assert.False(more);
        }

        [Fact]
        public void SinglePacket_ByteByByte_AccumulatesUntilComplete()
        {
            var (reader, _, hello, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            global::TrueCraft.API.Networking.IPacket finalPacket = null;
            for (int i = 0; i < hello.Length; i++)
            {
                processor.ProcessNextSegment(hello, i, 1, out var packet);
                if (packet != null) finalPacket = packet;
            }

            var chat = Assert.IsType<ChatMessagePacket>(finalPacket);
            Assert.Equal("hello", chat.Message);
        }

        [Fact]
        public void UnknownPacketId_Throws()
        {
            var (reader, _, _, _) = Setup();
            var processor = new PacketSegmentProcessor(reader, serverBound: true);

            // 0xAB is not a registered server-bound packet ID in beta 1.7.3.
            var garbage = new byte[] { 0xAB, 0x00, 0x00 };

            Assert.Throws<System.NotSupportedException>(() =>
                processor.ProcessNextSegment(garbage, 0, garbage.Length, out _));
        }
    }
}
