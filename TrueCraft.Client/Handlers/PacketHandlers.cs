using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TrueCraft.API;
using TrueCraft.API.Networking;
using TrueCraft.Client.Events;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Client.Handlers
{
    internal class PacketHandlers
    {
        private readonly ILogger<PacketHandlers> _log;

        public PacketHandlers(ILogger<PacketHandlers> log)
        {
            _log = log;
        }

        public void RegisterHandlers(MultiplayerClient client)
        {
            client.RegisterPacketHandler(new HandshakeResponsePacket().ID, HandleHandshake);
            client.RegisterPacketHandler(new ChatMessagePacket().ID, HandleChatMessage);
            client.RegisterPacketHandler(new SetPlayerPositionPacket().ID, HandlePositionAndLook);
            client.RegisterPacketHandler(new LoginResponsePacket().ID, HandleLoginResponse);
            client.RegisterPacketHandler(new UpdateHealthPacket().ID, HandleUpdateHealth);
            client.RegisterPacketHandler(new TimeUpdatePacket().ID, HandleTimeUpdate);

            client.RegisterPacketHandler(new ChunkPreamblePacket().ID, ChunkHandlers.HandleChunkPreamble);
            client.RegisterPacketHandler(new ChunkDataPacket().ID, ChunkHandlers.HandleChunkData);
            client.RegisterPacketHandler(new BlockChangePacket().ID, ChunkHandlers.HandleBlockChange);

            client.RegisterPacketHandler(new WindowItemsPacket().ID, InventoryHandlers.HandleWindowItems);
            client.RegisterPacketHandler(new SetSlotPacket().ID, InventoryHandlers.HandleSetSlot);
            client.RegisterPacketHandler(new CloseWindowPacket().ID, InventoryHandlers.HandleCloseWindowPacket);
            client.RegisterPacketHandler(new OpenWindowPacket().ID, InventoryHandlers.HandleOpenWindowPacket);
        }

        public void HandleChatMessage(IPacket packet, MultiplayerClient client)
        {
            var chatMessagePacket = (ChatMessagePacket) packet;
            client.OnChatMessage(new ChatMessageEventArgs(chatMessagePacket.Message));
        }

        public void HandleHandshake(IPacket packet, MultiplayerClient client)
        {
            var handshakeResponsePacket = (HandshakeResponsePacket) packet;
            if (handshakeResponsePacket.ConnectionHash != "-")
            {
                _log.LogError("Online mode is not supported");
                Process.GetCurrentProcess().Kill();
            }

            // TODO: Authentication
            client.QueuePacket(new LoginRequestPacket(PacketReader.Version, client.User.Username));
        }

        public void HandleLoginResponse(IPacket packet, MultiplayerClient client)
        {
            var loginResponsePacket = (LoginResponsePacket) packet;
            client.EntityID = loginResponsePacket.EntityID;
            client.QueuePacket(new PlayerGroundedPacket());
        }

        public void HandlePositionAndLook(IPacket packet, MultiplayerClient client)
        {
            var setPlayerPositionPacket = (SetPlayerPositionPacket) packet;
            client._Position = new Vector3(setPlayerPositionPacket.X, setPlayerPositionPacket.Y,
                setPlayerPositionPacket.Z);
            client.QueuePacket(setPlayerPositionPacket);
            client.LoggedIn = true;
            // TODO: Pitch and yaw
        }

        public void HandleUpdateHealth(IPacket packet, MultiplayerClient client)
        {
            var updateHealthPacket = (UpdateHealthPacket) packet;
            client.Health = updateHealthPacket.Health;
        }

        public void HandleTimeUpdate(IPacket packet, MultiplayerClient client)
        {
            var timeUpdatePacket = (TimeUpdatePacket) packet;
            var time = timeUpdatePacket.Time / 20.0;
            client.World.World.BaseTime = DateTime.UtcNow - TimeSpan.FromSeconds(time);
        }
    }
}
