using TrueCraft.Nbt.Tags;

namespace TrueCraft.Nbt.Serialization
{
    public interface INbtSerializable
    {
        NbtTag Serialize(string tagName);
        void Deserialize(NbtTag value);
    }
}