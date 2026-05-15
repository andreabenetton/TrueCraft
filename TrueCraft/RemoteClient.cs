using System;
using TrueCraft.API.Networking;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using TrueCraft.Core.Networking;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.API.Entities;
using TrueCraft.API;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;
using System.Threading.Tasks;
using System.Threading;
using TrueCraft.Core.Entities;
using System.IO;
using TrueCraft.Nbt;
using TrueCraft.API.Logging;
using TrueCraft.API.Logic;
using TrueCraft.Exceptions;
using TrueCraft.Nbt.Tags;
using TrueCraft.Core.Profiling;

namespace TrueCraft
{
    public class RemoteClient : IRemoteClient, IEventSubject, IDisposable
    {
        public RemoteClient(IMultiplayerServer server, IPacketReader packetReader, PacketHandler[] packetHandlers, Socket connection)
        {
            LoadedChunks = new HashSet<Coordinates2D>();
            Server = server;
            Inventory = new InventoryWindow(server.CraftingRepository);
            InventoryWindow.WindowChange += HandleWindowChange;
            SelectedSlot = InventoryWindow.HotbarIndex;
            CurrentWindow = InventoryWindow;
            ItemStaging = ItemStack.EmptyStack;
            KnownEntities = new List<IEntity>();
            Disconnected = false;
            EnableLogging = server.EnableClientLogging;
            NextWindowID = 1;
            Connection = connection;
            PacketReader = packetReader;
            PacketHandlers = packetHandlers;

            cancel = new CancellationTokenSource();

            // Phase N2: replaces the SAEA receive pump + per-client SemaphoreSlim with a Pipe and two
            // long-running async tasks. _fillTask reads from the socket into Writer; _processTask reads
            // back-out, parses packets via PacketReader.TryReadPacket, and awaits the registered handler.
            _receivePipe = new Pipe();
            _fillTask = Task.Run(() => FillReceivePipeAsync(cancel.Token));
            _processTask = Task.Run(() => ProcessReceivePipeAsync(cancel.Token));

            // Phase N3: queue + per-client send loop. Replaces the per-packet SocketAsyncEventArgs and
            // ToArray() allocations in QueuePacket. The channel is unbounded so QueuePacket never blocks;
            // multiple writers are fine (handlers, scheduler events, the tick loop), one reader is _sendTask.
            _sendQueue = Channel.CreateUnbounded<PendingSend>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            _sendTask = Task.Run(() => RunSendLoopAsync(cancel.Token));
        }

        public event EventHandler Disposed;

        /// <summary>
        /// A list of entities that this client is aware of.
        /// </summary>
        internal List<IEntity> KnownEntities { get; set; }
        internal sbyte NextWindowID { get; set; }

        //public NetworkStream NetworkStream { get; set; }
        public IMinecraftStream MinecraftStream { get; internal set; }
        public string Username { get; internal set; }
        public bool LoggedIn { get; internal set; }
        public IMultiplayerServer Server { get; set; }
        public IWorld World { get; internal set; }
        public IWindow Inventory { get; private set; }
        public short SelectedSlot { get; internal set; }
        public ItemStack ItemStaging { get; set; }
        public IWindow CurrentWindow { get; internal set; }
        public bool EnableLogging { get; set; }
        public DateTime ExpectedDigComplete { get; set; }

        public Socket Connection { get; private set; }

        // Phase N2: per-client receive pipeline. The Pipe replaces both the SAEA-based receive pump
        // and the old SemaphoreSlim(1,1) packet-handler serializer — the process task runs on a single
        // logical thread, so concurrent handler invocations for the same client are structurally impossible.
        private readonly Pipe _receivePipe;
        private readonly Task _fillTask;
        private readonly Task _processTask;

        // Phase N3: per-client send pipeline. QueuePacket pushes a (pooled buffer, length) onto the channel;
        // _sendTask drains and calls Socket.SendAsync on each entry, returning the buffer to ArrayPool.
        private readonly Channel<PendingSend> _sendQueue;
        private readonly Task _sendTask;

        private readonly struct PendingSend
        {
            public PendingSend(byte[] buffer, int length, bool isDisconnect)
            {
                Buffer = buffer;
                Length = length;
                IsDisconnect = isDisconnect;
            }
            public byte[] Buffer { get; }
            public int Length { get; }
            public bool IsDisconnect { get; }
        }

        public IPacketReader PacketReader { get; private set; }

        private PacketHandler[] PacketHandlers { get; set; }

        private IEntity _Entity;

        private long disconnected;

        private readonly CancellationTokenSource cancel;

        public bool Disconnected
        {
            get
            {
                return Interlocked.Read(ref disconnected) == 1;
            }
            internal set
            {
                Interlocked.CompareExchange(ref disconnected, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public IEntity Entity
        {
            get
            {
                return _Entity;
            }
            internal set
            {
                var player = _Entity as PlayerEntity;
                if (player != null)
                    player.PickUpItem -= HandlePickUpItem;
                _Entity = value;
                player = _Entity as PlayerEntity;
                if (player != null)
                    player.PickUpItem += HandlePickUpItem;
            }
        }

        void HandlePickUpItem(object sender, EntityEventArgs e)
        {
            var packet = new CollectItemPacket(e.Entity.EntityID, Entity.EntityID);
            QueuePacket(packet);
            var manager = Server.GetEntityManagerForWorld(World);
            foreach (var client in manager.ClientsForEntity(Entity))
                client.QueuePacket(packet);
            Inventory.PickUpStack((e.Entity as ItemEntity).Item);
        }

        public ItemStack SelectedItem
        {
            get
            {
                return Inventory[SelectedSlot];
            }
        }

        public InventoryWindow InventoryWindow
        {
            get
            {
                return Inventory as InventoryWindow;
            }
        }

        internal int ChunkRadius { get; set; }
        internal HashSet<Coordinates2D> LoadedChunks { get; set; }

        public bool DataAvailable
        {
            get
            {
                return true;
            }
        }

        public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "players", Username + ".nbt");
            if (Program.NodeConfiguration.Singleplayer)
                path = Path.Combine(((World)World).BaseDirectory, "player.nbt");
            if (!File.Exists(path))
                return false;
            try
            {
                var nbt = new NbtFile();
                await nbt.LoadFromFileAsync(path, cancellationToken).ConfigureAwait(false);
                Entity.Position = new Vector3(
                    nbt.RootTag["position"][0].DoubleValue,
                    nbt.RootTag["position"][1].DoubleValue,
                    nbt.RootTag["position"][2].DoubleValue);
                Inventory.SetSlots(((NbtList)nbt.RootTag["inventory"]).Select(t => ItemStack.FromNbt(t as NbtCompound)).ToArray());
                (Entity as PlayerEntity).Health = nbt.RootTag["health"].ShortValue;
                Entity.Yaw = nbt.RootTag["yaw"].FloatValue;
                Entity.Pitch = nbt.RootTag["pitch"].FloatValue;
            }
            catch { /* Who cares */ }
            return true;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "players", Username + ".nbt");
            if (Program.NodeConfiguration.Singleplayer)
                path = Path.Combine(((World)World).BaseDirectory, "player.nbt");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (Entity == null) // I didn't think this could happen but null reference exceptions have been repoted here
                return;
            var nbt = new NbtFile(new NbtCompound("player", new NbtTag[]
                {
                    new NbtString("username", Username),
                    new NbtList("position", new[]
                    {
                        new NbtDouble(Entity.Position.X),
                        new NbtDouble(Entity.Position.Y),
                        new NbtDouble(Entity.Position.Z)
                    }),
                    new NbtList("inventory", Inventory.GetSlots().Select(s => s.ToNbt())),
                    new NbtShort("health", (Entity as PlayerEntity).Health),
                    new NbtFloat("yaw", Entity.Yaw),
                    new NbtFloat("pitch", Entity.Pitch),
                }
            ));
            await nbt.SaveToFileAsync(path, NbtCompression.ZLib, cancellationToken).ConfigureAwait(false);
        }

        public void OpenWindow(IWindow window)
        {
            CurrentWindow = window;
            window.Client = this;
            window.ID = NextWindowID++;
            if (NextWindowID < 0) NextWindowID = 1;
            QueuePacket(new OpenWindowPacket(window.ID, window.Type, window.Name, (sbyte)window.MinecraftWasWrittenByFuckingIdiotsLength));
            QueuePacket(new WindowItemsPacket(window.ID, window.GetSlots()));
            window.WindowChange += HandleWindowChange;
        }

        public void CloseWindow(bool clientInitiated = false)
        {
            if (!clientInitiated)
                QueuePacket(new CloseWindowPacket(CurrentWindow.ID));
            CurrentWindow.CopyToInventory(Inventory);
            CurrentWindow.Dispose();
            CurrentWindow = InventoryWindow;
        }

        public void Log(string message, params object[] parameters)
        {
            if (EnableLogging)
                SendMessage(ChatColor.Gray + string.Format("[" + DateTime.UtcNow.ToShortTimeString() + "] " + message, parameters));
        }

        public void QueuePacket(IPacket packet)
        {
            if (Disconnected || (Connection != null && !Connection.Connected))
                return;

            // Serialize into a local MemoryStream, then copy into an ArrayPool-rented buffer that the
            // send loop will return after Connection.SendAsync completes. The old code allocated a new
            // SocketAsyncEventArgs and a fresh ToArray() byte[] *per packet*; both are now eliminated.
            using var writeStream = new MemoryStream();
            writeStream.WriteByte(packet.ID);
            var stream = new MinecraftStream(writeStream);
            packet.WritePacket(stream);

            int length = (int)writeStream.Length;
            byte[] rented = ArrayPool<byte>.Shared.Rent(length);
            writeStream.GetBuffer().AsSpan(0, length).CopyTo(rented);

            var item = new PendingSend(rented, length, packet is DisconnectPacket);
            if (!_sendQueue.Writer.TryWrite(item))
                ArrayPool<byte>.Shared.Return(rented);
        }

        private async Task RunSendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (await _sendQueue.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (_sendQueue.Reader.TryRead(out PendingSend item))
                    {
                        try
                        {
                            if (Connection != null && Connection.Connected)
                            {
                                await Connection.SendAsync(
                                    item.Buffer.AsMemory(0, item.Length),
                                    SocketFlags.None,
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }
                        catch (SocketException)
                        {
                            // peer gone — drain and return remaining buffers below
                        }
                        catch (ObjectDisposedException)
                        {
                            // socket disposed during shutdown
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(item.Buffer);
                        }

                        if (item.IsDisconnect)
                        {
                            Server.DisconnectClient(this);
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                Server.Log(LogCategory.Error, "Send loop failed: {0}", ex);
            }
            finally
            {
                // Return any buffers still queued so ArrayPool doesn't lose them.
                while (_sendQueue.Reader.TryRead(out PendingSend leftover))
                    ArrayPool<byte>.Shared.Return(leftover.Buffer);
            }
        }

        private void OperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            // After Phase N2 this only handles Send and Disconnect completions; receive moved to the Pipe.
            e.Completed -= OperationCompleted;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    IPacket packet = e.UserToken as IPacket;

                    if (packet is DisconnectPacket)
                        Server.DisconnectClient(this);

                    e.SetBuffer(null, 0, 0);
                    break;
                case SocketAsyncOperation.Disconnect:
                    Connection.Close();

                    break;
            }

            if (Connection != null)
                if (!Connection.Connected && !Disconnected)
                    Server.DisconnectClient(this);
        }

        // Hint for each ReceiveAsync — the pipe handles growth, but giving it a sane min-size
        // avoids tiny syscalls under burst load.
        private const int ReceiveBufferHint = 4 * 1024;

        private async Task FillReceivePipeAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Memory<byte> mem = _receivePipe.Writer.GetMemory(ReceiveBufferHint);
                    int read;
                    try
                    {
                        read = await Connection.ReceiveAsync(mem, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    if (read == 0)
                        break; // peer closed
                    _receivePipe.Writer.Advance(read);
                    FlushResult flush = await _receivePipe.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    if (flush.IsCompleted)
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                Server.Log(LogCategory.Error, "Receive pump failed: {0}", ex);
            }
            finally
            {
                await _receivePipe.Writer.CompleteAsync().ConfigureAwait(false);
                // Signal the process task to drain and exit, then make sure the client is disconnected.
                if (!Disconnected)
                    Server.DisconnectClient(this);
            }
        }

        private async Task ProcessReceivePipeAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadResult result;
                    try
                    {
                        result = await _receivePipe.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    ReadOnlySequence<byte> buffer = result.Buffer;
                    try
                    {
                        while (PacketReader.TryReadPacket(ref buffer, out IPacket packet))
                        {
                            var handler = PacketHandlers[packet.ID];
                            if (handler == null)
                            {
                                Log("Unhandled packet {0}", packet.GetType().Name);
                                continue;
                            }
                            try
                            {
                                await handler(packet, this, Server).ConfigureAwait(false);
                            }
                            catch (PlayerDisconnectException)
                            {
                                Server.DisconnectClient(this);
                                return;
                            }
                            catch (Exception ex)
                            {
                                Server.Log(LogCategory.Debug, "Disconnecting client due to exception in network worker");
                                Server.Log(LogCategory.Debug, ex.ToString());
                                Server.DisconnectClient(this);
                                return;
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        Server.Log(LogCategory.Debug, "Disconnecting client due to unsupported packet received.");
                        Server.DisconnectClient(this);
                        return;
                    }

                    // Tell the pipe what we consumed (buffer's start advanced past parsed packets)
                    // and that we examined everything we received this round (so the next ReadAsync
                    // only completes when more bytes arrive or the writer completes).
                    _receivePipe.Reader.AdvanceTo(buffer.Start, buffer.End);

                    if (result.IsCompleted)
                        break;
                }
            }
            finally
            {
                await _receivePipe.Reader.CompleteAsync().ConfigureAwait(false);
            }
        }

        public void Disconnect()
        {
            if (Disconnected)
                return;

            Disconnected = true;

            // Stop accepting new packets to send; the send loop drains anything already queued.
            _sendQueue.Writer.TryComplete();

            cancel.Cancel();

            try { Connection.Shutdown(SocketShutdown.Send); }
            catch (SocketException) { /* peer already gone */ }
            catch (ObjectDisposedException) { /* socket already disposed */ }

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OperationCompleted;
            try { Connection.DisconnectAsync(args); }
            catch (ObjectDisposedException) { /* already disposed */ }
        }

        public void SendMessage(string message)
        {
            var parts = message.Split('\n');
            foreach (var part in parts)
                QueuePacket(new ChatMessagePacket(part));
        }

        internal void ExpandChunkRadius(IMultiplayerServer server)
        {
            if (this.Disconnected)
                return;
            Task.Factory.StartNew(() =>
            {
                if (ChunkRadius < 8) // TODO: Allow customization of this number
                {
                    ChunkRadius++;
                    server.Scheduler.ScheduleEvent("client.update-chunks", this,
                        TimeSpan.Zero, s => UpdateChunks());
                    server.Scheduler.ScheduleEvent("remote.chunks", this,
                        TimeSpan.FromSeconds(1), ExpandChunkRadius);
                }
            });
        }

        internal void SendKeepAlive(IMultiplayerServer server)
        {
            QueuePacket(new KeepAlivePacket());
            server.Scheduler.ScheduleEvent("remote.keepalive", this, TimeSpan.FromSeconds(10), SendKeepAlive);
        }

        internal void UpdateChunks(bool block = false)
        {
            var newChunks = new HashSet<Coordinates2D>();
            var toLoad = new List<Tuple<Coordinates2D, IChunk>>();
            Profiler.Start("client.new-chunks");
            for (int x = -ChunkRadius; x < ChunkRadius; x++)
            {
                for (int z = -ChunkRadius; z < ChunkRadius; z++)
                {
                    var coords = new Coordinates2D(
                        ((int)Entity.Position.X >> 4) + x,
                        ((int)Entity.Position.Z >> 4) + z);
                    newChunks.Add(coords);
                    if (!LoadedChunks.Contains(coords))
                        toLoad.Add(new Tuple<Coordinates2D, IChunk>(
                            coords, World.GetChunk(coords, generate: block)));
                }
            }
            Profiler.Done();
            var encode = new Action(() =>
            {
                Profiler.Start("client.encode-chunks");
                foreach (var tup in toLoad)
                {
                    var coords = tup.Item1;
                    var chunk = tup.Item2;
                    if (chunk == null)
                        chunk = World.GetChunk(coords);
                    chunk.LastAccessed = DateTime.UtcNow;
                    LoadChunk(chunk);
                }
                Profiler.Done();
            });
            if (block)
                encode();
            else
                Task.Factory.StartNew(encode);
            Profiler.Start("client.old-chunks");
            LoadedChunks.IntersectWith(newChunks);
            Profiler.Done();
            Profiler.Start("client.update-entities");
            ((EntityManager)Server.GetEntityManagerForWorld(World)).UpdateClientEntities(this);
            Profiler.Done();
        }

        internal void UnloadAllChunks()
        {
            while (LoadedChunks.Any())
            {
                UnloadChunk(LoadedChunks.First());
            }
        }

        internal void LoadChunk(IChunk chunk)
        {
            QueuePacket(new ChunkPreamblePacket(chunk.Coordinates.X, chunk.Coordinates.Z));
            QueuePacket(CreatePacket(chunk));
            Server.Scheduler.ScheduleEvent("client.finalize-chunks", this,
                TimeSpan.Zero, server =>
                {
                    // Finalize step is intentionally empty: the original logic added the
                    // chunk to LoadedChunks and registered tile entities, but was disabled
                    // (preceded by `return;`) while the chunk-load lifecycle is being
                    // reworked. Removed to silence CS0162; git history preserves the body.
                });
        }

        internal void UnloadChunk(Coordinates2D position)
        {
            QueuePacket(new ChunkPreamblePacket(position.X, position.Z, false));
            LoadedChunks.Remove(position);
        }

        void HandleWindowChange(object sender, WindowChangeEventArgs e)
        {
            if (!(sender is InventoryWindow))
            {
                QueuePacket(new SetSlotPacket((sender as IWindow).ID, (short)e.SlotIndex, e.Value.ID, e.Value.Count, e.Value.Metadata));
                return;
            }

            QueuePacket(new SetSlotPacket(0, (short)e.SlotIndex, e.Value.ID, e.Value.Count, e.Value.Metadata));

            if (e.SlotIndex == SelectedSlot)
            {
                var notified = Server.GetEntityManagerForWorld(World).ClientsForEntity(Entity);
                foreach (var c in notified)
                    c.QueuePacket(new EntityEquipmentPacket(Entity.EntityID, 0, SelectedItem.ID, SelectedItem.Metadata));
            }
            if (e.SlotIndex >= InventoryWindow.ArmorIndex && e.SlotIndex < InventoryWindow.ArmorIndex + InventoryWindow.Armor.Length)
            {
                short slot = (short)(4 - (e.SlotIndex - InventoryWindow.ArmorIndex));
                var notified = Server.GetEntityManagerForWorld(World).ClientsForEntity(Entity);
                foreach (var c in notified)
                    c.QueuePacket(new EntityEquipmentPacket(Entity.EntityID, slot, e.Value.ID, e.Value.Metadata));
            }
        }

        private static ChunkDataPacket CreatePacket(IChunk chunk)
        {
            var X = chunk.Coordinates.X;
            var Z = chunk.Coordinates.Z;

            Profiler.Start("client.encode-chunks.compress");
            byte[] result;
            using (var ms = new MemoryStream())
            {
                using (var deflate = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                    deflate.Write(chunk.Data, 0, chunk.Data.Length);
                result = ms.ToArray();
            }
            Profiler.Done();

            return new ChunkDataPacket(X * Chunk.Width, 0, Z * Chunk.Depth,
                Chunk.Width, Chunk.Height, Chunk.Depth, result);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();

                if (Disposed != null)
                    Disposed(this, null);
            }
        }
    }
}
