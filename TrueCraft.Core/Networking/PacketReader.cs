using System;
using System.Buffers;
using System.IO;
using System.Linq.Expressions;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Networking
{
    public class PacketReader : IPacketReader
    {
        public static readonly int Version = 14;

        internal Func<IPacket>[] ClientboundPackets = new Func<IPacket>[0x100];
        internal Func<IPacket>[] ServerboundPackets = new Func<IPacket>[0x100];

        public int ProtocolVersion => Version;

        public void RegisterPacketType<T>(bool clientbound = true, bool serverbound = true) where T : IPacket
        {
            var func = Expression.Lambda<Func<IPacket>>(Expression.Convert(Expression.New(typeof(T)), typeof(IPacket)))
                .Compile();
            var packet = func();

            if (clientbound)
                ClientboundPackets[packet.ID] = func;
            if (serverbound)
                ServerboundPackets[packet.ID] = func;
        }

        public void WritePacket(IMinecraftStream stream, IPacket packet)
        {
            stream.WriteUInt8(packet.ID);
            packet.WritePacket(stream);
            stream.BaseStream.Flush();
        }

        // Hint for the contiguous scratch buffer used by TryReadPacket. Beta 1.7.3 chunk-data packets
        // are the largest legitimate packets (raw is bounded by Chunk.Width*Height*Depth*2.5 ~= 80 KiB);
        // 256 KiB gives us comfortable headroom without becoming a memory hog.
        private const int MaxPacketSize = 256 * 1024;

        public bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out IPacket packet, bool serverbound = true)
        {
            packet = null;
            if (buffer.IsEmpty) return false;

            // Snapshot up to MaxPacketSize bytes into a contiguous rented buffer, then try to parse
            // one packet via the existing MinecraftStream / IPacket.ReadPacket machinery. If parsing
            // runs out of bytes, the packet wants more — leave the sequence untouched and return false.
            int candidateLen = (int)Math.Min(buffer.Length, MaxPacketSize);
            byte[] rented = ArrayPool<byte>.Shared.Rent(candidateLen);
            try
            {
                buffer.Slice(0, candidateLen).CopyTo(rented);
                byte packetId = rented[0];
                var factory = serverbound ? ServerboundPackets[packetId] : ClientboundPackets[packetId];
                if (factory == null)
                    throw new NotSupportedException("Unable to read packet type 0x" + packetId.ToString("X2"));

                using var ms = new MemoryStream(rented, 1, candidateLen - 1);
                using var stream = new MinecraftStream(ms);
                IPacket p = factory();
                try
                {
                    p.ReadPacket(stream);
                }
                catch (EndOfStreamException)
                {
                    return false;
                }
                int consumed = 1 + (int)ms.Position; // +1 for the packet ID byte
                buffer = buffer.Slice(consumed);
                packet = p;
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        ///     Registers TrueCraft.Core implementations of all packets used by vanilla Minecraft.
        /// </summary>
        public void RegisterCorePackets()
        {
            RegisterPacketType<KeepAlivePacket>(); // 0x00
            RegisterPacketType<LoginRequestPacket>(serverbound: true, clientbound: false); // 0x01
            RegisterPacketType<LoginResponsePacket>(serverbound: false); // 0x01
            RegisterPacketType<HandshakePacket>(serverbound: true, clientbound: false); // 0x02
            RegisterPacketType<HandshakeResponsePacket>(serverbound: false); // 0x02
            RegisterPacketType<ChatMessagePacket>(); // 0x03
            RegisterPacketType<TimeUpdatePacket>(serverbound: false); // 0x04
            RegisterPacketType<EntityEquipmentPacket>(serverbound: false); // 0x05 // NOTE: serverbound not confirmed
            RegisterPacketType<SpawnPositionPacket>(serverbound: false); // 0x06
            RegisterPacketType<UseEntityPacket>(serverbound: true, clientbound: false); // 0x07
            RegisterPacketType<UpdateHealthPacket>(serverbound: false); // 0x08
            RegisterPacketType<RespawnPacket>(); // 0x09
            RegisterPacketType<PlayerGroundedPacket>(serverbound: true, clientbound: false); // 0x0A
            RegisterPacketType<PlayerPositionPacket>(serverbound: true, clientbound: false); // 0x0B
            RegisterPacketType<PlayerLookPacket>(serverbound: true, clientbound: false); // 0x0C
            RegisterPacketType<PlayerPositionAndLookPacket>(serverbound: true, clientbound: false); // 0x0D
            RegisterPacketType<SetPlayerPositionPacket>(serverbound: false); // 0x0D
            RegisterPacketType<PlayerDiggingPacket>(serverbound: true, clientbound: false); // 0x0E
            RegisterPacketType<PlayerBlockPlacementPacket>(serverbound: true, clientbound: false); // 0x0F
            RegisterPacketType<ChangeHeldItemPacket>(serverbound: true, clientbound: false); // 0x10
            RegisterPacketType<UseBedPacket>(serverbound: false); // 0x11
            RegisterPacketType<AnimationPacket>(); // 0x12
            RegisterPacketType<PlayerActionPacket>(serverbound: true, clientbound: false); // 0x13
            RegisterPacketType<SpawnPlayerPacket>(serverbound: false); // 0x14
            RegisterPacketType<SpawnItemPacket>(); // 0x15
            RegisterPacketType<CollectItemPacket>(serverbound: false); // 0x16
            RegisterPacketType<SpawnGenericEntityPacket>(serverbound: false); // 0x17
            RegisterPacketType<SpawnMobPacket>(serverbound: false); // 0x18
            RegisterPacketType<SpawnPaintingPacket>(serverbound: false); // 0x19

            RegisterPacketType<EntityVelocityPacket>(serverbound: false); // 0x1C
            RegisterPacketType<DestroyEntityPacket>(serverbound: false); // 0x1D
            RegisterPacketType<UselessEntityPacket>(serverbound: false); // 0x1E
            RegisterPacketType<EntityRelativeMovePacket>(serverbound: false); // 0x1F
            RegisterPacketType<EntityLookPacket>(serverbound: false); // 0x20
            RegisterPacketType<EntityLookAndRelativeMovePacket>(serverbound: false); // 0x21
            RegisterPacketType<EntityTeleportPacket>(serverbound: false); // 0x22

            RegisterPacketType<EntityStatusPacket>(serverbound: false); // 0x26
            RegisterPacketType<AttachEntityPacket>(serverbound: false); // 0x27
            RegisterPacketType<EntityMetadataPacket>(serverbound: false); // 0x28

            RegisterPacketType<ChunkPreamblePacket>(serverbound: false); // 0x32
            RegisterPacketType<ChunkDataPacket>(serverbound: false); // 0x33
            RegisterPacketType<BulkBlockChangePacket>(serverbound: false); // 0x34
            RegisterPacketType<BlockChangePacket>(serverbound: false); // 0x35
            RegisterPacketType<BlockActionPacket>(serverbound: false); // 0x36

            RegisterPacketType<ExplosionPacket>(serverbound: false); // 0x3C
            RegisterPacketType<SoundEffectPacket>(serverbound: false); // 0x3D

            RegisterPacketType<EnvironmentStatePacket>(serverbound: false); // 0x46
            RegisterPacketType<LightningPacket>(serverbound: false); // 0x47

            RegisterPacketType<OpenWindowPacket>(serverbound: false); // 0x64
            RegisterPacketType<CloseWindowPacket>(); // 0x65
            RegisterPacketType<ClickWindowPacket>(serverbound: true, clientbound: false); // 0x66
            RegisterPacketType<SetSlotPacket>(serverbound: false); // 0x67
            RegisterPacketType<WindowItemsPacket>(serverbound: false); // 0x68
            RegisterPacketType<UpdateProgressPacket>(serverbound: false); // 0x69
            RegisterPacketType<TransactionStatusPacket>(serverbound: false); // 0x6A

            RegisterPacketType<UpdateSignPacket>(); // 0x82
            RegisterPacketType<MapDataPacket>(serverbound: false); // 0x83

            RegisterPacketType<UpdateStatisticPacket>(serverbound: false); // 0xC8

            RegisterPacketType<DisconnectPacket>(); // 0xFF
        }
    }
}