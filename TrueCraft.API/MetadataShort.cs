using TrueCraft.API.Networking;

namespace TrueCraft.API
{
    public class MetadataShort : MetadataEntry
    {
        public short Value;

        public MetadataShort()
        {
        }

        public MetadataShort(short value)
        {
            Value = value;
        }

        public override byte Identifier => 1;
        public override string FriendlyName => "short";

        public static implicit operator MetadataShort(short value)
        {
            return new MetadataShort(value);
        }

        public override void FromStream(IMinecraftStream stream)
        {
            Value = stream.ReadInt16();
        }

        public override void WriteTo(IMinecraftStream stream, byte index)
        {
            stream.WriteUInt8(GetKey(index));
            stream.WriteInt16(Value);
        }
    }
}