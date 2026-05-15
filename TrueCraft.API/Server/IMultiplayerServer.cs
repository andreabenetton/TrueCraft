using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.API.Server
{
    /// <summary>
    ///     Called when the given packet comes in from a remote client. Awaited by the dispatch loop; synchronous
    ///     handlers should return Task.CompletedTask.
    /// </summary>
    public delegate Task PacketHandler(IPacket packet, IRemoteClient client, IMultiplayerServer server);

    public interface IMultiplayerServer
    {
        IAccessConfiguration AccessConfiguration { get; }
        IPacketReader PacketReader { get; }
        IList<IRemoteClient> Clients { get; }
        IList<IWorld> Worlds { get; }
        IEventScheduler Scheduler { get; }
        IBlockRepository BlockRepository { get; }
        ICraftingRepository CraftingRepository { get; }
        IItemRepository ItemRepository { get; }
        IPEndPoint EndPoint { get; }
        bool BlockUpdatesEnabled { get; set; }
        bool EnableClientLogging { get; set; }
        event EventHandler<ChatMessageEventArgs> ChatMessageReceived;
        event EventHandler<PlayerJoinedQuitEventArgs> PlayerJoined;
        event EventHandler<PlayerJoinedQuitEventArgs> PlayerQuit;

        void Start(IPEndPoint endPoint);
        void Stop();
        void RegisterPacketHandler(byte packetId, PacketHandler handler);
        void AddWorld(IWorld world);
        IEntityManager GetEntityManagerForWorld(IWorld world);
        void SendMessage(string message, params object[] parameters);

        void DisconnectClient(IRemoteClient client);

        bool PlayerIsWhitelisted(string client);
        bool PlayerIsBlacklisted(string client);
        bool PlayerIsOp(string client);
    }
}