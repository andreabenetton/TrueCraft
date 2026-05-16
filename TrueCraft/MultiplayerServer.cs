using System;
using TrueCraft.API.Server;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.World;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Options;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.World;
using System.Diagnostics;
using System.Collections.Concurrent;
using TrueCraft.Core.Profiling;

namespace TrueCraft;

public class MultiplayerServer : IMultiplayerServer, IDisposable
{
    public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;
    public event EventHandler<PlayerJoinedQuitEventArgs> PlayerJoined;
    public event EventHandler<PlayerJoinedQuitEventArgs> PlayerQuit;

    public IAccessConfiguration AccessConfiguration { get; internal set; }

    public IPacketReader PacketReader { get; private set; }
    public IList<IRemoteClient> Clients { get; private set; }
    public IList<IWorld> Worlds { get; private set; }
    public IList<IEntityManager> EntityManagers { get; private set; }
    public IList<WorldLighting> WorldLighters { get; set; }
    public IEventScheduler Scheduler { get; private set; }
    public IBlockRepository BlockRepository { get; private set; }
    public IItemRepository ItemRepository { get; private set; }
    public ICraftingRepository CraftingRepository { get; private set; }
    public bool EnableClientLogging { get; set; }
    public IPEndPoint EndPoint { get; private set; }

    private static readonly int MillisecondsPerTick = 1000 / 20;

    private bool _BlockUpdatesEnabled = true;

    private struct BlockUpdate
    {
        public Coordinates3D Coordinates;
        public IWorld World;
    }
    private Queue<BlockUpdate> PendingBlockUpdates { get; set; }
    public bool BlockUpdatesEnabled
    {
        get
        {
            return _BlockUpdatesEnabled;
        }
        set
        {
            _BlockUpdatesEnabled = value;
            if (_BlockUpdatesEnabled)
            {
                ProcessBlockUpdates();
            }
        }
    }

    // Tick loop is driven by a PeriodicTimer + async task (see RunEnvironmentLoopAsync). Phase 6 replaced the
    // System.Threading.Timer-based EnvironmentWorker so tick callbacks can `await` without re-entering on a
    // pool thread.
    private PeriodicTimer EnvironmentTimer;
    private CancellationTokenSource EnvironmentCts;
    private Task EnvironmentLoopTask;
    // Accept loop (Phase N4): TAP-based `await listener.AcceptSocketAsync(ct)` in a while loop. The old
    // SAEA-recursion accept pattern is gone; accept failures are now logged instead of silently swallowed.
    private CancellationTokenSource AcceptCts;
    private Task AcceptLoopTask;
    private TcpListener Listener;
    private readonly PacketHandler[] PacketHandlers;
    private Stopwatch Time;
    private ConcurrentBag<Tuple<IWorld, IChunk>> ChunksToSchedule;
    internal object ClientLock = new object();
    
    private QueryProtocol QueryProtocol;

    internal bool ShuttingDown { get; private set; }
    
    private readonly NodeOptions _node;
    private readonly ILogger<MultiplayerServer> Log;
    private readonly Profiler Profiler;

    public MultiplayerServer(IBlockRepository blockRepository, IItemRepository itemRepository,
        ICraftingRepository craftingRepository, IOptions<NodeOptions> nodeOpts,
        IOptions<AccessOptions> accessOpts, Handlers.LoginHandlers loginHandlers,
        ILogger<MultiplayerServer> log, Profiler profiler)
    {
        _node = nodeOpts.Value;
        Log = log;
        Profiler = profiler;
        var reader = new PacketReader();
        PacketReader = reader;
        Clients = new List<IRemoteClient>();
        PacketHandlers = new PacketHandler[0x100];
        Worlds = new List<IWorld>();
        EntityManagers = new List<IEntityManager>();
        Scheduler = new EventScheduler(this, profiler);
        BlockRepository = blockRepository;
        ItemRepository = itemRepository;
        BlockProvider.ItemRepository = ItemRepository;
        BlockProvider.BlockRepository = BlockRepository;
        CraftingRepository = craftingRepository;
        PendingBlockUpdates = new Queue<BlockUpdate>();
        EnableClientLogging = false;
        QueryProtocol = ActivatorUtilities.CreateInstance<QueryProtocol>(App.Services, this);
        WorldLighters = new List<WorldLighting>();
        ChunksToSchedule = new ConcurrentBag<Tuple<IWorld, IChunk>>();
        Time = new Stopwatch();

        AccessConfiguration = accessOpts.Value;

        reader.RegisterCorePackets();
        Handlers.PacketHandlers.RegisterHandlers(this, loginHandlers);
    }

    public void RegisterPacketHandler(byte packetId, PacketHandler handler)
    {
        PacketHandlers[packetId] = handler;
    }

    public void Start(IPEndPoint endPoint)
    {
        Scheduler.DisabledEvents.Clear();
        if (_node.DisabledEvents is not null)
            _node.DisabledEvents.ToList().ForEach(
                ev => Scheduler.DisabledEvents.Add(ev));
        ShuttingDown = false;
        Time.Reset();
        Time.Start();
        Listener = new TcpListener(endPoint);
        Listener.Start();
        EndPoint = (IPEndPoint)Listener.LocalEndpoint;

        AcceptCts = new CancellationTokenSource();
        AcceptLoopTask = Task.Run(() => RunAcceptLoopAsync(AcceptCts.Token));

        Log.LogInformation("Running TrueCraft server on {EndPoint}", EndPoint);
        EnvironmentCts = new CancellationTokenSource();
        EnvironmentTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(MillisecondsPerTick));
        EnvironmentLoopTask = Task.Run(() => RunEnvironmentLoopAsync(EnvironmentCts.Token));
        if(_node.Query)
            QueryProtocol.Start();
    }

    public void Stop()
    {
        ShuttingDown = true;
        // Cancel the accept loop *before* Listener.Stop() so a pending AcceptSocketAsync sees cancellation
        // rather than ObjectDisposedException — easier to filter at the catch site.
        try
        {
            AcceptCts?.Cancel();
            Listener.Stop();
            AcceptLoopTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
        {
            // expected on shutdown
        }
        if(_node.Query)
            QueryProtocol.Stop();
        // Stop the tick loop before saving worlds: the loop must not be running concurrently with disposal.
        try
        {
            EnvironmentCts?.Cancel();
            EnvironmentTimer?.Dispose();
            EnvironmentLoopTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
        {
            // expected on shutdown
        }
        foreach (var w in Worlds)
            w.Save();
        foreach (var c in Clients)
            DisconnectClient(c);
    }

    public void AddWorld(IWorld world)
    {
        Worlds.Add(world);
        world.BlockRepository = BlockRepository;
        world.ChunkGenerated += HandleChunkGenerated;
        world.ChunkLoaded += HandleChunkLoaded;
        world.BlockChanged += HandleBlockChanged;
        var manager = new EntityManager(this, world);
        EntityManagers.Add(manager);
        var lighter = new WorldLighting(world, BlockRepository, Profiler);
        WorldLighters.Add(lighter);
        foreach (var chunk in world)
            HandleChunkLoaded(world, new ChunkLoadedEventArgs(chunk));
    }

    void HandleChunkLoaded(object sender, ChunkLoadedEventArgs e)
    {
        if (_node.EnableEventLoading)
            ChunksToSchedule.Add(new Tuple<IWorld, IChunk>(sender as IWorld, e.Chunk));
        if (_node.EnableLighting)
        {
            var lighter = WorldLighters.SingleOrDefault(l => l.World == sender);
            lighter.InitialLighting(e.Chunk, false);
        }
    }

    void HandleBlockChanged(object sender, BlockChangeEventArgs e)
    {
        // TODO: Propegate lighting changes to client (not possible with beta 1.7.3 protocol)
        if (e.NewBlock.ID != e.OldBlock.ID || e.NewBlock.Metadata != e.OldBlock.Metadata)
        {
            for (int i = 0, ClientsCount = Clients.Count; i < ClientsCount; i++)
            {
                var client = (RemoteClient)Clients[i];
                // TODO: Confirm that the client knows of this block
                if (client.LoggedIn && client.World == sender)
                {
                    client.QueuePacket(new BlockChangePacket(e.Position.X, (sbyte)e.Position.Y, e.Position.Z,
                            (sbyte)e.NewBlock.ID, (sbyte)e.NewBlock.Metadata));
                }
            }
            PendingBlockUpdates.Enqueue(new BlockUpdate { Coordinates = e.Position, World = sender as IWorld });
            ProcessBlockUpdates();
            if (_node.EnableLighting)
            {
                var lighter = WorldLighters.SingleOrDefault(l => l.World == sender);
                if (lighter is not null)
                {
                    var posA = e.Position;
                    posA.Y = 0;
                    var posB = e.Position;
                    posB.Y = World.Height;
                    posB.X++; posB.Z++;
                    lighter.EnqueueOperation(new BoundingBox(posA, posB), true);
                    lighter.EnqueueOperation(new BoundingBox(posA, posB), false);
                }
            }
        }
    }

    void HandleChunkGenerated(object sender, ChunkLoadedEventArgs e)
    {
        if (_node.EnableLighting)
        {
            var lighter = new WorldLighting(sender as IWorld, BlockRepository, Profiler);
            lighter.InitialLighting(e.Chunk, false);
        }
        else
        {
            for (int i = 0; i < e.Chunk.SkyLight.Length * 2; i++)
            {
                e.Chunk.SkyLight[i] = 0xF;
            }
        }
        HandleChunkLoaded(sender, e);
    }

    void ScheduleUpdatesForChunk(IWorld world, IChunk chunk)
    {
        chunk.UpdateHeightMap();
        int _x = chunk.Coordinates.X * Chunk.Width;
        int _z = chunk.Coordinates.Z * Chunk.Depth;
        Coordinates3D coords, _coords;
        for (byte x = 0; x < Chunk.Width; x++)
        {
            for (byte z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < chunk.GetHeight(x, z); y++)
                {
                    _coords.X = x; _coords.Y = y; _coords.Z = z;
                    var id = chunk.GetBlockID(_coords);
                    if (id == 0)
                        continue;
                    coords.X = _x + x; coords.Y = y; coords.Z = _z + z;
                    var provider = BlockRepository.GetBlockProvider(id);
                    provider.BlockLoadedFromChunk(coords, this, world);
                }
            }
        }
    }

    private void ProcessBlockUpdates()
    {
        if (!BlockUpdatesEnabled)
            return;
        var adjacent = new[]
        {
            Coordinates3D.Up, Coordinates3D.Down,
            Coordinates3D.Left, Coordinates3D.Right,
            Coordinates3D.Forwards, Coordinates3D.Backwards
        };
        while (PendingBlockUpdates.Count != 0)
        {
            var update = PendingBlockUpdates.Dequeue();
            var source = update.World.GetBlockData(update.Coordinates);
            foreach (var offset in adjacent)
            {
                var descriptor = update.World.GetBlockData(update.Coordinates + offset);
                var provider = BlockRepository.GetBlockProvider(descriptor.ID);
                if (provider is not null)
                    provider.BlockUpdate(descriptor, source, this, update.World);
            }
        }
    }

    public IEntityManager GetEntityManagerForWorld(IWorld world)
    {
        for (int i = 0; i < EntityManagers.Count; i++)
        {
            var manager = EntityManagers[i] as EntityManager;
            if (manager.World == world)
                return manager;
        }
        return null;
    }

    public void SendMessage(string message, params object[] parameters)
    {
        var compiled = string.Format(message, parameters);
        var parts = compiled.Split('\n');
        foreach (var client in Clients)
        {
            foreach (var part in parts)
                client.SendMessage(part);
        }
        Log.LogInformation("{Message}", ChatColor.RemoveColors(compiled));
    }

    protected internal void OnChatMessageReceived(ChatMessageEventArgs e)
    {
        if (ChatMessageReceived is not null)
            ChatMessageReceived(this, e);
    }

    protected internal void OnPlayerJoined(PlayerJoinedQuitEventArgs e)
    {
        if (PlayerJoined is not null)
            PlayerJoined(this, e);
    }

    protected internal void OnPlayerQuit(PlayerJoinedQuitEventArgs e)
    {
        if (PlayerQuit is not null)
            PlayerQuit(this, e);
    }

    public void DisconnectClient(IRemoteClient _client)
    {
        var client = (RemoteClient)_client;

        lock (ClientLock)
        {
            Clients.Remove(client);
        }

        if (client.Disconnected)
            return;

        client.Disconnected = true;

        if (client.LoggedIn)
        {
            SendMessage(ChatColor.Yellow + "{0} has left the server.", client.Username);
            AuditLog.PlayerLeft(client.Username);
            GetEntityManagerForWorld(client.World).DespawnEntity(client.Entity);
            GetEntityManagerForWorld(client.World).FlushDespawns();
        }
        // Transitional bridge: DisconnectClient is still sync (IMultiplayerServer surface unchanged in Phase 4);
        // we sync-wait the async save here. Phase 8a will await this on the async dispatch path.
        client.SaveAsync().GetAwaiter().GetResult();
        client.Disconnect();
        OnPlayerQuit(new PlayerJoinedQuitEventArgs(client));

        client.Dispose();
    }

    private async Task RunAcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                socket = await Listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                // Listener.Stop() called concurrently — that's the shutdown path.
                break;
            }
            catch (SocketException ex)
            {
                // Transient errors (file descriptor exhaustion, peer aborted before accept, etc.).
                // Log and continue accepting; don't bring the whole server down on a single accept failure.
                Log.LogError(ex, "Accept failed");
                continue;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Accept failed unexpectedly");
                continue;
            }

            try
            {
                var client = ActivatorUtilities.CreateInstance<RemoteClient>(
                    App.Services, this, PacketReader, PacketHandlers, socket);
                lock (ClientLock)
                    Clients.Add(client);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Client setup failed for {RemoteEndPoint}",
                    socket.RemoteEndPoint?.ToString() ?? "<unknown>");
                try { socket.Close(); } catch (ObjectDisposedException) { }
            }
        }
    }

    private async Task RunEnvironmentLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await EnvironmentTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (ShuttingDown)
                    break;
                try
                {
                    await DoEnvironmentAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "Environment tick raised");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }


    private async Task DoEnvironmentAsync(CancellationToken cancellationToken)
    {
        if (ShuttingDown)
            return;

        long limit = Time.ElapsedMilliseconds + MillisecondsPerTick;
        Profiler.Start("environment");

        await Scheduler.UpdateAsync(cancellationToken).ConfigureAwait(false);

        Profiler.Start("environment.entities");
        foreach (var manager in EntityManagers)
        {
            manager.Update();
        }
        Profiler.Done();

        if (_node.EnableLighting)
        {
            Profiler.Start("environment.lighting");
            foreach (var lighter in WorldLighters)
            {
                while (Time.ElapsedMilliseconds < limit && lighter.TryLightNext())
                {
                    // This space intentionally left blank
                }
                if (Time.ElapsedMilliseconds >= limit)
                    Log.LogWarning("Lighting queue is backed up");
            }
            Profiler.Done();
        }

        if (_node.EnableEventLoading)
        {
            Profiler.Start("environment.chunks");
            Tuple<IWorld, IChunk> t;
            if (ChunksToSchedule.TryTake(out t))
                ScheduleUpdatesForChunk(t.Item1, t.Item2);
            Profiler.Done();
        }

        Profiler.Done(MillisecondsPerTick);
    }

    public bool PlayerIsWhitelisted(string client)
    {
        return AccessConfiguration.Whitelist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
    }

    public bool PlayerIsBlacklisted(string client)
    {
        return AccessConfiguration.Blacklist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
    }

    public bool PlayerIsOp(string client)
    {
        return AccessConfiguration.Oplist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
        }
    }

    ~MultiplayerServer()
    {
        Dispose(false);
    }
}
