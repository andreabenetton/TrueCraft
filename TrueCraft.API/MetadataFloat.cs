using TrueCraft.API.Networking;

namespace TrueCraft.API
{
    public class MetadataFloat : MetadataEntry
    {
        public float Value;

        public MetadataFloat()
        {
        }

        public MetadataFloat(float value)
        {
            Value = value;
        }

        public override byte Identifier => 3;
        public override string FriendlyName => "float";

        public static implicit operator MetadataFloat(float value)
        {
            return new MetadataFloat(value);
        }

        public override void FromStream(IMinecraftStream stream)
        {
            Value = stream.ReadSingle();
        }

        public override void WriteTo(IMinecraftStream stream, byte index)
        {
            stream.WriteUInt8(GetKey(index));
            stream.WriteSingle(Value);
        }
    }
}