namespace TrueCraft.Core.Entities
{
    public abstract class ObjectEntity : Entity
    {
        public abstract byte EntityType { get; }
        public abstract int Data { get; }
    }
}
