using TrueCraft.API.Networking;

namespace TrueCraft.API
{
    public class MetadataInt : MetadataEntry
    {
        public int Value;

        public MetadataInt()
        {
        }

        public MetadataInt(int value)
        {
            Value = value;
        }

        public override byte Identifier => 2;
        public override string FriendlyName => "int";

        public static implicit operator MetadataInt(int value)
        {
            return new MetadataInt(value);
        }

        public override void FromStream(IMinecraftStream stream)
        {
            Value = stream.ReadInt32();
        }

        public override void WriteTo(IMinecraftStream stream, byte index)
        {
            stream.WriteUInt8(GetKey(index));
            stream.WriteInt32(Value);
        }
    }
}