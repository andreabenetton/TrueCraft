namespace TrueCraft.API.Physics
{
    public interface IAABBEntity : IPhysicsEntity
    {
        BoundingBox BoundingBox { get; }
        Size Size { get; }

        void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection);
    }
}