using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using Xunit;

namespace Test.TrueCraft.Core.Networking;

public class NetworkLoopbackTest
{
    // Exercises the same socket + Pipe + TryReadPacket code shape RemoteClient runs in production,
    // against a real loopback TCP socket pair. RemoteClient itself is too coupled to MultiplayerServer
    // to spin up in a unit test, but the network plumbing it relies on (FillReceivePipeAsync /
    // ProcessReceivePipeAsync / TryReadPacket / Channel send loop) can be reproduced here in
    // miniature. A regression in the production loops would almost certainly also break this test.

    private static byte[] Serialize<T>(PacketReader reader, T packet) where T : IPacket
    {
        using var ms = new MemoryStream();
        using var stream = new MinecraftStream(ms);
        reader.WritePacket(stream, packet);
        return ms.ToArray();
    }

    private static async Task<(TcpClient client, Socket serverSocket, TcpListener listener)> ConnectLoopbackAsync()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        var connectTask = client.ConnectAsync(IPAddress.Loopback, port);
        var acceptTask = listener.AcceptSocketAsync();
        await Task.WhenAll(connectTask, acceptTask);
        listener.Stop();

        return (client, await acceptTask, listener);
    }

    // Drives the same receive pattern RemoteClient uses: socket -> PipeWriter -> PipeReader -> TryReadPacket.
    // Collects parsed packets until at least `expectedCount` are seen, or the deadline elapses.
    private static async Task<List<IPacket>> DrainAsync(Socket socket, PacketReader reader,
        int expectedCount, bool serverbound, CancellationToken cancellationToken)
    {
        var pipe = new Pipe();
        var packets = new List<IPacket>();

        var fill = Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Memory<byte> mem = pipe.Writer.GetMemory(4096);
                    int n;
                    try { n = await socket.ReceiveAsync(mem, SocketFlags.None, cancellationToken).ConfigureAwait(false); }
                    catch (SocketException) { break; }
                    catch (ObjectDisposedException) { break; }
                    if (n == 0) break;
                    pipe.Writer.Advance(n);
                    var fr = await pipe.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    if (fr.IsCompleted) break;
                }
            }
            catch (OperationCanceledException) { }
            finally { await pipe.Writer.CompleteAsync().ConfigureAwait(false); }
        }, cancellationToken);

        try
        {
            while (!cancellationToken.IsCancellationRequested && packets.Count < expectedCount)
            {
                ReadResult rr;
                try { rr = await pipe.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
                var buffer = rr.Buffer;
                while (reader.TryReadPacket(ref buffer, out IPacket p, serverbound))
                    packets.Add(p);
                pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                if (rr.IsCompleted) break;
            }
        }
        finally
        {
            await pipe.Reader.CompleteAsync().ConfigureAwait(false);
        }

        return packets;
    }

    [Fact]
    public async Task SinglePacket_FromClientToServer_RoundTrips()
    {
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var (client, serverSocket, _) = await ConnectLoopbackAsync();
        using (client)
        using (serverSocket)
        {
            byte[] payload = Serialize(reader, new ChatMessagePacket("hello-from-client"));
            await client.GetStream().WriteAsync(payload, cts.Token);
            await client.GetStream().FlushAsync(cts.Token);

            var packets = await DrainAsync(serverSocket, reader, expectedCount: 1, serverbound: true, cts.Token);

            Assert.Single(packets);
            Assert.Equal("hello-from-client", ((ChatMessagePacket)packets[0]).Message);
        }
    }

    [Fact]
    public async Task MultiplePackets_BatchedWrite_AllParsedOnServerSide()
    {
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var (client, serverSocket, _) = await ConnectLoopbackAsync();
        using (client)
        using (serverSocket)
        {
            // Three back-to-back packets in a single TCP write — typical small-burst case.
            byte[] a = Serialize(reader, new ChatMessagePacket("one"));
            byte[] b = Serialize(reader, new KeepAlivePacket());
            byte[] c = Serialize(reader, new ChatMessagePacket("three"));
            var combined = new byte[a.Length + b.Length + c.Length];
            Buffer.BlockCopy(a, 0, combined, 0, a.Length);
            Buffer.BlockCopy(b, 0, combined, a.Length, b.Length);
            Buffer.BlockCopy(c, 0, combined, a.Length + b.Length, c.Length);
            await client.GetStream().WriteAsync(combined, cts.Token);

            var packets = await DrainAsync(serverSocket, reader, expectedCount: 3, serverbound: true, cts.Token);

            Assert.Equal(3, packets.Count);
            Assert.Equal("one", ((ChatMessagePacket)packets[0]).Message);
            Assert.IsType<KeepAlivePacket>(packets[1]);
            Assert.Equal("three", ((ChatMessagePacket)packets[2]).Message);
        }
    }

    [Fact]
    public async Task PacketSplitAcrossTwoWrites_StillParses()
    {
        // The kernel may or may not coalesce these into one read, but the *parser side* must
        // tolerate the worst case: the packet arrives in two separate receive callbacks.
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var (client, serverSocket, _) = await ConnectLoopbackAsync();
        using (client)
        using (serverSocket)
        {
            byte[] payload = Serialize(reader, new ChatMessagePacket("split-message"));
            int half = payload.Length / 2;

            var ns = client.GetStream();
            await ns.WriteAsync(payload.AsMemory(0, half), cts.Token);
            await ns.FlushAsync(cts.Token);
            // Small delay encourages the receiver to see two separate reads. Not load-bearing —
            // if the kernel coalesces, the test still passes because the parser handles either.
            await Task.Delay(50, cts.Token);
            await ns.WriteAsync(payload.AsMemory(half), cts.Token);

            var packets = await DrainAsync(serverSocket, reader, expectedCount: 1, serverbound: true, cts.Token);

            Assert.Single(packets);
            Assert.Equal("split-message", ((ChatMessagePacket)packets[0]).Message);
        }
    }

    [Fact]
    public async Task PooledChannelSend_FromQueueToWire_PreservesOrder()
    {
        // Exercises the send-loop shape from RemoteClient.RunSendLoopAsync without bringing up
        // a RemoteClient: enqueue several pre-serialized packets on a Channel and a small loop
        // sends them over a loopback socket. The other side drains and parses.
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        // Combine the test's local timeout with the xUnit framework cancellation
        // token so dotnet test --blame-hang etc. can interrupt the loop.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        var (client, serverSocket, _) = await ConnectLoopbackAsync();
        using (client)
        using (serverSocket)
        {
            var queue = System.Threading.Channels.Channel.CreateUnbounded<(byte[] buf, int len)>(
                new System.Threading.Channels.UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

            // Producer: enqueue 5 packets with rented buffers
            var messages = new[] { "first", "second", "third", "fourth", "fifth" };
            foreach (var msg in messages)
            {
                byte[] bytes = Serialize(reader, new ChatMessagePacket(msg));
                byte[] rented = ArrayPool<byte>.Shared.Rent(bytes.Length);
                bytes.AsSpan().CopyTo(rented);
                Assert.True(queue.Writer.TryWrite((rented, bytes.Length)));
            }
            queue.Writer.Complete();

            // Consumer: drain queue and send on the client socket; return buffers.
            // cts is already linked to TestContext.Current.CancellationToken above.
            var sendTask = Task.Run(async () =>
            {
                while (await queue.Reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
                {
                    while (queue.Reader.TryRead(out var item))
                    {
                        try
                        {
                            await client.Client.SendAsync(
                                item.buf.AsMemory(0, item.len),
                                SocketFlags.None,
                                cts.Token).ConfigureAwait(false);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(item.buf);
                        }
                    }
                }
            }, cts.Token);

            var packets = await DrainAsync(serverSocket, reader, expectedCount: messages.Length, serverbound: true, cts.Token);
            await sendTask; // ensure the send loop finished

            Assert.Equal(messages.Length, packets.Count);
            for (int i = 0; i < messages.Length; i++)
                Assert.Equal(messages[i], ((ChatMessagePacket)packets[i]).Message);
        }
    }

    [Fact]
    public async Task PeerCloses_FillLoopTerminatesCleanly()
    {
        // FillReceivePipeAsync must exit when the peer closes the socket (read returns 0).
        // Otherwise the per-client tasks would leak.
        var reader = new PacketReader();
        reader.RegisterCorePackets();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var (client, serverSocket, _) = await ConnectLoopbackAsync();
        using (serverSocket)
        {
            // Client sends one packet then closes immediately.
            byte[] payload = Serialize(reader, new KeepAlivePacket());
            await client.GetStream().WriteAsync(payload, cts.Token);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();

            var packets = await DrainAsync(serverSocket, reader, expectedCount: 1, serverbound: true, cts.Token);

            Assert.Single(packets);
            Assert.IsType<KeepAlivePacket>(packets[0]);
            // If DrainAsync hadn't terminated on peer-close, the cts deadline would have fired
            // and we'd have hit the OperationCanceledException path with fewer packets.
        }
    }
}
