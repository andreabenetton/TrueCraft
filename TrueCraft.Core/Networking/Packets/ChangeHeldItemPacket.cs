using TrueCraft.API.Networking;

namespace TrueCraft.Core.Networking.Packets
{
    public struct ChangeHeldItemPacket : IPacket
    {
        public byte ID => 0x10;

        public short Slot;

        public void ReadPacket(IMinecraftStream stream)
        {
            Slot = stream.ReadInt16();
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt16(Slot);
        }
    }
}