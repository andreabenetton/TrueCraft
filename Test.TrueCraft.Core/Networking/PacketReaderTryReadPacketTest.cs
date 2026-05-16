using System;
using System.Buffers;
using System.IO;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Core.Networking;

public class PacketReaderTryReadPacketTest
{
    // Tests for IPacketReader.TryReadPacket(ref ReadOnlySequence<byte>, out IPacket, bool).
    // The new sequence-based parser must satisfy the same framing invariants the old
    // PacketSegmentProcessor did: it consumes only the bytes one packet needs, leaves the
    // rest for the next call, and reports "need more" without advancing on an incomplete packet.

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
    public void EmptySequence_ReturnsFalse()
    {
        var (reader, _, _, _) = Setup();
        var buffer = new ReadOnlySequence<byte>([]);

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.False(ok);
        Assert.Null(packet);
        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void WholePacket_Single_ConsumesAndYields()
    {
        var (reader, hello, _, _) = Setup();
        var buffer = new ReadOnlySequence<byte>(hello);

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.True(ok);
        Assert.Equal("hello", ((ChatMessagePacket)packet).Message);
        Assert.Equal(0, buffer.Length); // exactly hello consumed
    }

    [Fact]
    public void PartialPacket_ReturnsFalseAndLeavesBufferIntact()
    {
        var (reader, hello, _, _) = Setup();
        var half = hello.Length / 2;
        var slice = new byte[half];
        Array.Copy(hello, slice, half);
        var buffer = new ReadOnlySequence<byte>(slice);

        var ok = reader.TryReadPacket(ref buffer, out var packet);

        Assert.False(ok);
        Assert.Null(packet);
        Assert.Equal(half, buffer.Length); // not consumed
    }

    [Fact]
    public void TwoBackToBackPackets_ConsumesOneAtATime()
    {
        var (reader, hello, world, _) = Setup();
        var combined = new byte[hello.Length + world.Length];
        System.Buffer.BlockCopy(hello, 0, combined, 0, hello.Length);
        System.Buffer.BlockCopy(world, 0, combined, hello.Length, world.Length);
        var buffer = new ReadOnlySequence<byte>(combined);

        Assert.True(reader.TryReadPacket(ref buffer, out var first));
        Assert.Equal("hello", ((ChatMessagePacket)first).Message);
        Assert.Equal(world.Length, buffer.Length);

        Assert.True(reader.TryReadPacket(ref buffer, out var second));
        Assert.Equal("world", ((ChatMessagePacket)second).Message);
        Assert.Equal(0, buffer.Length);

        Assert.False(reader.TryReadPacket(ref buffer, out var none));
        Assert.Null(none);
    }

    [Fact]
    public void HeterogeneousPackets_KeepAliveThenChat_BothParsed()
    {
        var (reader, hello, _, keepAlive) = Setup();
        var combined = new byte[keepAlive.Length + hello.Length];
        System.Buffer.BlockCopy(keepAlive, 0, combined, 0, keepAlive.Length);
        System.Buffer.BlockCopy(hello, 0, combined, keepAlive.Length, hello.Length);
        var buffer = new ReadOnlySequence<byte>(combined);

        Assert.True(reader.TryReadPacket(ref buffer, out var ka));
        Assert.IsType<KeepAlivePacket>(ka);

        Assert.True(reader.TryReadPacket(ref buffer, out var chat));
        Assert.Equal("hello", ((ChatMessagePacket)chat).Message);

        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void UnknownPacketId_Throws()
    {
        var (reader, _, _, _) = Setup();
        var buffer = new ReadOnlySequence<byte>(new byte[] { 0xAB, 0x00, 0x00 });

        Assert.Throws<NotSupportedException>(() =>
        {
            reader.TryReadPacket(ref buffer, out _);
        });
    }
}
