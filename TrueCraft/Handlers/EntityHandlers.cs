using System.Threading.Tasks;
using TrueCraft.API.Networking;
using TrueCraft.API.Server;
using TrueCraft.API;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Handlers
{
    internal static class EntityHandlers
    {
        public static Task HandlePlayerPositionPacket(IPacket _packet, IRemoteClient _client, IMultiplayerServer server)
        {
            var packet = (PlayerPositionPacket)_packet;
            HandlePlayerMovement(_client, new Vector3(packet.X, packet.Y, packet.Z), _client.Entity.Yaw, _client.Entity.Pitch);
            return Task.CompletedTask;
        }

        public static Task HandlePlayerLookPacket(IPacket _packet, IRemoteClient _client, IMultiplayerServer server)
        {
            var packet = (PlayerLookPacket)_packet;
            HandlePlayerMovement(_client, _client.Entity.Position, packet.Yaw, packet.Pitch);
            return Task.CompletedTask;
        }

        public static Task HandlePlayerPositionAndLookPacket(IPacket _packet, IRemoteClient _client, IMultiplayerServer server)
        {
            var packet = (PlayerPositionAndLookPacket)_packet;
            HandlePlayerMovement(_client, new Vector3(packet.X, packet.Y, packet.Z), packet.Yaw, packet.Pitch);
            return Task.CompletedTask;
        }

        public static void HandlePlayerMovement(IRemoteClient client, Vector3 position, float yaw, float pitch)
        {
            if (client.Entity.Position.DistanceTo(position) > 10)
            {
                //client.QueuePacket(new DisconnectPacket("Client moved to fast (hacking?) " + client.Entity.Position.DistanceTo(position)));
                // TODO: Figure out a good way of knowing when the client is going to stop BSing us about its position
                client.Entity.Position = position;
                client.Entity.Yaw = yaw;
                client.Entity.Pitch = pitch;
            }
            else
            {
                client.Entity.Position = position;
                client.Entity.Yaw = yaw;
                client.Entity.Pitch = pitch;
            }
        }
    }
}