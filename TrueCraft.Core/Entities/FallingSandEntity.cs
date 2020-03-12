using System.Linq;
using TrueCraft.API;
using TrueCraft.API.Networking;
using TrueCraft.API.Physics;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Entities
{
    public class FallingSandEntity : ObjectEntity, IAABBEntity
    {
        public FallingSandEntity(Vector3 position)
        {
            _Position = position + new Vector3(0.5);
        }

        public override byte EntityType => 70;

        public override IPacket SpawnPacket =>
            new SpawnGenericEntityPacket(EntityID, (sbyte) EntityType,
                MathHelper.CreateAbsoluteInt(Position.X), MathHelper.CreateAbsoluteInt(Position.Y),
                MathHelper.CreateAbsoluteInt(Position.Z), 0, null, null, null);

        public override int Data => 1;
        public override Size Size => new Size(0.98);

        public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            if (Despawned)
                return;
            if (collisionDirection == Vector3.Down)
            {
                var id = SandBlock.BlockID;
                if (EntityType == 71)
                    id = GravelBlock.BlockID;
                EntityManager.DespawnEntity(this);
                var position = (Coordinates3D) collisionPoint + Coordinates3D.Up;
                var hit = World.BlockRepository.GetBlockProvider(World.GetBlockID(position));
                if (hit.BoundingBox == null && BlockProvider.Overwritable.All(o => o != hit.ID))
                    EntityManager.SpawnEntity(new ItemEntity(position + new Vector3(0.5), new ItemStack(id)));
                else
                    World.SetBlockID(position, id);
            }
        }

        public BoundingBox BoundingBox => new BoundingBox(Position - Size / 2, Position + Size / 2);

        public bool BeginUpdate()
        {
            EnablePropertyChange = false;
            return true;
        }

        public void EndUpdate(Vector3 newPosition)
        {
            EnablePropertyChange = true;
            Position = newPosition;
        }

        public float AccelerationDueToGravity => 0.8f;
        public float Drag => 0.40f;
        public float TerminalVelocity => 39.2f;
    }
}