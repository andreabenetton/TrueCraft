using TrueCraft.API;
using TrueCraft.API.Networking;

namespace TrueCraft.Core.Networking.Packets
{
    public struct EntityMetadataPacket : IPacket
    {
        public byte ID => 0x28;

        public int EntityID;
        public MetadataDictionary Metadata;

        public EntityMetadataPacket(int entityID, MetadataDictionary metadata)
        {
            EntityID = entityID;
            Metadata = metadata;
        }

        public void ReadPacket(IMinecraftStream stream)
        {
            EntityID = stream.ReadInt32();
            Metadata = MetadataDictionary.FromStream(stream);
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt32(EntityID);
            Metadata.WriteTo(stream);
        }
    }
}