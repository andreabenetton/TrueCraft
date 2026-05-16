using System;
using System.Buffers;
using System.IO;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Core.Networking;

public class PacketReaderMultiSegmentTest
{
    // PacketReaderTryReadPacketTest covers contiguous buffers (new ReadOnlySequence<byte>(byte[])).
    // In production, System.IO.Pipelines often hands back a *fragmented* ReadOnlySequence — multiple
    // ReadOnlyMemory<byte> segments linked together. TryReadPacket must handle that path correctly;
    // a regression here would manifest as 'protocol works in unit tests, breaks against real clients'.

    private static byte[] Serialize<T>(PacketReader reader, T packet) where T : IPacket
    {
        using var ms = new MemoryStream();
        using var stream = new MinecraftStream(ms);
        reader.WritePacket(stream, packet);
        return ms.ToArray();
    }

    // Minimal helper to build a multi-segment ReadOnlySequence<byte> from N byte chunks.
    private sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(ReadOnlyMemory<byte> memory) { Memory = memory; }
        public Segment Append(ReadOnlyMemory<byte> memory)
        {
            var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }

    private static ReadOnlySequence<byte> MakeMultiSegment(params byte[][] chunks)
    {
        if (chunks.Length == 0) return ReadOnlySequence<byte>.Empty;
        var first = new Segment(chunks[0]);
        var last = first;
        for (int i = 1; i < chunks.Length; i++)
            last = last.Append(chunks[i]);
        return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
    }

    [Fact]
    public void SinglePacket_AcrossThreeSegments_Parses()
    {
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        byte[] hello = Serialize(reader, new ChatMessagePacket("hello"));

        // Split hello into 3 unequal segments to make sure the parser doesn't assume contiguity.
        int s1 = hello.Length / 3;
        int s2 = hello.Length / 2 - s1;
        var seg1 = hello.AsMemory(0, s1).ToArray();
        var seg2 = hello.AsMemory(s1, s2).ToArray();
        var seg3 = hello.AsMemory(s1 + s2).ToArray();

        var buffer = MakeMultiSegment(seg1, seg2, seg3);
        Assert.False(buffer.IsSingleSegment); // sanity: actually multi-segment

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.True(ok);
        Assert.Equal("hello", ((ChatMessagePacket)packet).Message);
        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void TwoPackets_OnePerSegment_BothParse()
    {
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        byte[] hello = Serialize(reader, new ChatMessagePacket("hello"));
        byte[] world = Serialize(reader, new ChatMessagePacket("world"));

        var buffer = MakeMultiSegment(hello, world);
        Assert.False(buffer.IsSingleSegment);

        Assert.True(reader.TryReadPacket(ref buffer, out var first));
        Assert.Equal("hello", ((ChatMessagePacket)first).Message);
        // After consuming hello, the remaining sequence may or may not be single-segment depending
        // on whether hello.Length aligned exactly with a segment boundary. For two-segment input
        // where the first segment is exactly `hello`, the remainder is the second segment alone.

        Assert.True(reader.TryReadPacket(ref buffer, out var second));
        Assert.Equal("world", ((ChatMessagePacket)second).Message);
        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void PacketSpanningSegmentBoundary_ParsesCleanly()
    {
        // The packet straddles two segments: first segment carries packet ID + 1 byte of payload,
        // second segment carries the rest. This is the worst-case for naive 'check buffer.First' code.
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        byte[] hello = Serialize(reader, new ChatMessagePacket("hello"));

        var seg1 = hello.AsMemory(0, 2).ToArray();    // [0x03, length_hi]
        var seg2 = hello.AsMemory(2).ToArray();        // [length_lo, ...string bytes...]
        var buffer = MakeMultiSegment(seg1, seg2);

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.True(ok);
        Assert.Equal("hello", ((ChatMessagePacket)packet).Message);
        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void IncompletePacket_AcrossSegments_ReturnsFalseLeavesBufferIntact()
    {
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        byte[] hello = Serialize(reader, new ChatMessagePacket("hello"));

        // Two segments, total ~half a packet — definitely incomplete.
        int firstHalf = hello.Length / 4;
        int secondHalf = hello.Length / 4;
        var seg1 = hello.AsMemory(0, firstHalf).ToArray();
        var seg2 = hello.AsMemory(firstHalf, secondHalf).ToArray();
        var buffer = MakeMultiSegment(seg1, seg2);
        long lengthBefore = buffer.Length;

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.False(ok);
        Assert.Null(packet);
        Assert.Equal(lengthBefore, buffer.Length); // unchanged
    }
}
