namespace TrueCraft.Nbt
{
    // Represents state of a node in the NBT file tree, used by NbtWriter
    internal sealed class NbtWriterNode
    {
        public int ListIndex;
        public int ListSize;
        public NbtTagType ListType;
        public NbtTagType ParentType;
    }
}