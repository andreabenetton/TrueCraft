using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using TrueCraft.Client;
using TrueCraft.Client.Handlers;
using TrueCraft.Core;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Client;

public class ChunkHandlersMarshallingTest
{
    private static PacketHandlers NullHandlers() =>
        new PacketHandlers(NullLogger<PacketHandlers>.Instance);

    [Fact]
    public void BlockChangeIsDeferredWhenMarshallerIsSet()
    {
        var client = new MultiplayerClient(new TrueCraftUser { Username = "test" }, NullHandlers());
        var deferred = new List<Action>();
        client.MainThreadInvoke = deferred.Add;

        var packet = new BlockChangePacket(x: 0, y: 0, z: 0, blockID: 1, metadata: 0);
        ChunkHandlers.HandleBlockChange(packet, client);

        Assert.Single(deferred);
    }

    [Fact]
    public void ChunkPreambleIsDeferredWhenMarshallerIsSet()
    {
        var client = new MultiplayerClient(new TrueCraftUser { Username = "test" }, NullHandlers());
        var deferred = new List<Action>();
        client.MainThreadInvoke = deferred.Add;

        var packet = new ChunkPreamblePacket { X = 0, Z = 0 };
        ChunkHandlers.HandleChunkPreamble(packet, client);

        Assert.Single(deferred);
    }

    [Fact]
    public void HandlerRunsInlineWhenNoMarshallerIsSet()
    {
        var client = new MultiplayerClient(new TrueCraftUser { Username = "test" }, NullHandlers());
        // MainThreadInvoke deliberately null.

        // With no chunk loaded, the inner action returns silently.
        // The point of the test is that we don't throw NullReferenceException.
        var packet = new BlockChangePacket(x: 0, y: 0, z: 0, blockID: 1, metadata: 0);
        ChunkHandlers.HandleBlockChange(packet, client);
    }
}
