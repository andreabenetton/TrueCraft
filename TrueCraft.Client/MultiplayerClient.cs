using System;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.Physics;
using TrueCraft.API.Windows;
using TrueCraft.API.World;
using TrueCraft.Client.Events;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Physics;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client
{
    public delegate void PacketHandler(IPacket packet, MultiplayerClient client);

    public class
        MultiplayerClient : IAABBEntity, INotifyPropertyChanged, IDisposable // TODO: Make IMultiplayerClient and so on
    {
        private readonly CancellationTokenSource cancel;

        private readonly PacketHandler[] PacketHandlers;

        private long _connected;
        private int _hotbarSelection;

        // Phase N5: per-client receive pipeline mirrors the server. The Pipe replaces the SAEA receive
        // pump and the old SemaphoreSlim(cancel.Token) (which blocked the UI on a slow handler indefinitely).
        private readonly Pipe _receivePipe = new Pipe();
        private Task _fillTask;
        private Task _processTask;

        public MultiplayerClient(TrueCraftUser user)
        {
            User = user;
            Client = new TcpClient();
            PacketReader = new PacketReader();
            PacketReader.RegisterCorePackets();
            PacketHandlers = new PacketHandler[0x100];
            Handlers.PacketHandlers.RegisterHandlers(this);
            World = new ReadOnlyWorld();
            Inventory = new InventoryWindow(null);
            var repo = new BlockRepository();
            repo.DiscoverBlockProviders();
            World.World.BlockRepository = repo;
            World.World.ChunkProvider = new EmptyGenerator();
            Physics = new PhysicsEngine(World.World, repo);
            SocketPool = new SocketAsyncEventArgsPool(100, 200, 65536);
            _connected = 0;
            cancel = new CancellationTokenSource();
            Health = 20;
            var crafting = new CraftingRepository();
            CraftingRepository = crafting;
            crafting.DiscoverRecipes();
        }

        public TrueCraftUser User { get; set; }
        public ReadOnlyWorld World { get; }
        public PhysicsEngine Physics { get; set; }
        public bool LoggedIn { get; internal set; }
        public int EntityID { get; internal set; }
        public InventoryWindow Inventory { get; set; }
        public int Health { get; set; }
        public IWindow CurrentWindow { get; set; }
        public ICraftingRepository CraftingRepository { get; set; }

        public bool Connected => Interlocked.Read(ref _connected) == 1;

        public int HotbarSelection
        {
            get => _hotbarSelection;
            set
            {
                _hotbarSelection = value;
                QueuePacket(new ChangeHeldItemPacket {Slot = (short) value});
            }
        }

        private TcpClient Client { get; }
        private IMinecraftStream Stream { get; set; }
        private PacketReader PacketReader { get; }

        private SocketAsyncEventArgsPool SocketPool { get; }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ChatMessageEventArgs> ChatMessage;
        public event EventHandler<ChunkEventArgs> ChunkModified;
        public event EventHandler<ChunkEventArgs> ChunkLoaded;
        public event EventHandler<ChunkEventArgs> ChunkUnloaded;
        public event EventHandler<BlockChangeEventArgs> BlockChanged;

        public void RegisterPacketHandler(byte packetId, PacketHandler handler)
        {
            PacketHandlers[packetId] = handler;
        }

        public void Connect(IPEndPoint endPoint)
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += Connection_Completed;
            args.RemoteEndPoint = endPoint;

            if (!Client.Client.ConnectAsync(args))
                Connection_Completed(this, args);
        }

        private void Connection_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Interlocked.CompareExchange(ref _connected, 1, 0);

                Physics.AddEntity(this);

                _fillTask = Task.Run(() => FillReceivePipeAsync(cancel.Token));
                _processTask = Task.Run(() => ProcessReceivePipeAsync(cancel.Token));
                QueuePacket(new HandshakePacket(User.Username));
            }
            else
            {
                throw new Exception("Could not connect to server!");
            }
        }

        public void Disconnect()
        {
            if (!Connected)
                return;

            QueuePacket(new DisconnectPacket("Disconnecting"));

            Interlocked.CompareExchange(ref _connected, 0, 1);
        }

        public void SendMessage(string message)
        {
            var parts = message.Split('\n');
            foreach (var part in parts)
                QueuePacket(new ChatMessagePacket(part));
        }

        public void QueuePacket(IPacket packet)
        {
            if (!Connected || Client != null && !Client.Connected)
                return;

            using (var writeStream = new MemoryStream())
            {
                using (var ms = new MinecraftStream(writeStream))
                {
                    ms.WriteUInt8(packet.ID);
                    packet.WritePacket(ms);
                }

                var buffer = writeStream.ToArray();

                var args = new SocketAsyncEventArgs();
                args.UserToken = packet;
                args.Completed += OperationCompleted;
                args.SetBuffer(buffer, 0, buffer.Length);

                if (Client != null && !Client.Client.SendAsync(args))
                    OperationCompleted(this, args);
            }
        }

        private void OperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            // After Phase N5, only Send completions reach this handler — receive moved to the Pipe.
            e.Completed -= OperationCompleted;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    var packet = e.UserToken as IPacket;

                    if (packet is DisconnectPacket)
                    {
                        Client.Client.Shutdown(SocketShutdown.Send);
                        Client.Close();

                        cancel.Cancel();
                    }

                    e.SetBuffer(null, 0, 0);
                    break;
            }
        }

        // Hint for each ReceiveAsync — same rationale as the server side.
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
                        read = await Client.Client.ReceiveAsync(mem, SocketFlags.None, cancellationToken)
                            .ConfigureAwait(false);
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
                        break; // server closed
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
            finally
            {
                await _receivePipe.Writer.CompleteAsync().ConfigureAwait(false);
                if (Connected)
                    Disconnect();
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
                        while (PacketReader.TryReadPacket(ref buffer, out IPacket packet, serverbound: false))
                        {
                            if (PacketHandlers.Length > packet.ID && PacketHandlers[packet.ID] != null)
                                PacketHandlers[packet.ID](packet, this);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // Unrecognized packet from server — disconnect cleanly.
                        Disconnect();
                        return;
                    }

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

        protected internal void OnChatMessage(ChatMessageEventArgs e)
        {
            ChatMessage?.Invoke(this, e);
        }

        protected internal void OnChunkLoaded(ChunkEventArgs e)
        {
            ChunkLoaded?.Invoke(this, e);
        }

        protected internal void OnChunkUnloaded(ChunkEventArgs e)
        {
            ChunkUnloaded?.Invoke(this, e);
        }

        protected internal void OnChunkModified(ChunkEventArgs e)
        {
            ChunkModified?.Invoke(this, e);
        }

        protected internal void OnBlockChanged(BlockChangeEventArgs e)
        {
            BlockChanged?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
            }
        }

        ~MultiplayerClient()
        {
            Dispose(false);
        }

        #region IAABBEntity implementation

        private const double Width = 0.6;
        private const double Height = 1.62;
        private const double Depth = 0.6;

        public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            // This space intentionally left blank
        }

        public BoundingBox BoundingBox
        {
            get
            {
                var pos = Position - new Vector3(Width / 2, 0, Depth / 2);
                return new BoundingBox(pos, pos + Size);
            }
        }

        public Size Size => new Size(Width, Height, Depth);

        #endregion

        #region IPhysicsEntity implementation

        public bool BeginUpdate()
        {
            return true;
        }

        public void EndUpdate(Vector3 newPosition)
        {
            Position = newPosition;
        }

        public float Yaw { get; set; }
        public float Pitch { get; set; }

        internal Vector3 _Position;

        public Vector3 Position
        {
            get => _Position;
            set
            {
                if (_Position != value)
                {
                    QueuePacket(new PlayerPositionAndLookPacket(value.X, value.Y, value.Y + Height,
                        value.Z, Yaw, Pitch, false));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
                }

                _Position = value;
            }
        }

        public Vector3 Velocity { get; set; }

        public float AccelerationDueToGravity => 1.6f;

        public float Drag => 0.40f;

        public float TerminalVelocity => 78.4f;

        #endregion
    }
}