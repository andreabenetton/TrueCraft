using TrueCraft.API.Networking;
using TrueCraft.Nbt;

namespace TrueCraft.API
{
    public class MetadataSlot : MetadataEntry
    {
        public ItemStack Value;

        public MetadataSlot()
        {
        }

        public MetadataSlot(ItemStack value)
        {
            Value = value;
        }

        public override byte Identifier => 5;
        public override string FriendlyName => "slot";

        public static implicit operator MetadataSlot(ItemStack value)
        {
            return new MetadataSlot(value);
        }

        public override void FromStream(IMinecraftStream stream)
        {
            Value = ItemStack.FromStream(stream);
        }

        public override void WriteTo(IMinecraftStream stream, byte index)
        {
            stream.WriteUInt8(GetKey(index));
            stream.WriteInt16(Value.ID);
            if (Value.ID != -1)
            {
                stream.WriteInt8(Value.Count);
                stream.WriteInt16(Value.Metadata);
                if (Value.Nbt != null)
                {
                    var file = new NbtFile(Value.Nbt);
                    var data = file.SaveToBuffer(NbtCompression.GZip);
                    stream.WriteInt16((short) data.Length);
                    stream.WriteUInt8Array(data);
                }
                else
                {
                    stream.WriteInt16(-1);
                }
            }
        }
    }
}