using System;
using System.Linq;
using TrueCraft.API;
using TrueCraft.API.Networking;
using TrueCraft.API.Physics;
using TrueCraft.API.Server;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Entities
{
    public class ItemEntity : ObjectEntity, IAABBEntity
    {
        public static float PickupRange = 2;

        public ItemEntity(Vector3 position, ItemStack item)
        {
            Position = position;
            Item = item;
            Velocity = new Vector3(MathHelper.Random.NextDouble() * 0.25 - 0.125, 0.25,
                MathHelper.Random.NextDouble() * 0.25 - 0.125);
            if (item.Empty)
                Despawned = true;
        }

        public ItemStack Item { get; set; }

        public override IPacket SpawnPacket =>
            new SpawnItemPacket(EntityID, Item.ID, Item.Count, Item.Metadata,
                MathHelper.CreateAbsoluteInt(Position.X), MathHelper.CreateAbsoluteInt(Position.Y),
                MathHelper.CreateAbsoluteInt(Position.Z),
                MathHelper.CreateRotationByte(Yaw),
                MathHelper.CreateRotationByte(Pitch), 0);

        public override byte EntityType => 2;

        public override int Data => 1;

        public override MetadataDictionary Metadata
        {
            get
            {
                var metadata = base.Metadata;
                metadata[10] = Item;
                return metadata;
            }
        }

        public override bool SendMetadataToClients => false;

        public override Size Size => new Size(0.25, 0.25, 0.25);

        public BoundingBox BoundingBox => new BoundingBox(Position, Position + Size);

        public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            if (collisionDirection == Vector3.Down) Velocity = Vector3.Zero;
        }

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

        public float AccelerationDueToGravity => 1.98f;

        public float Drag => 0.40f;

        public float TerminalVelocity => 39.2f;

        public override void Update(IEntityManager entityManager)
        {
            var nearbyEntities = entityManager.EntitiesInRange(Position, PickupRange);
            if ((DateTime.UtcNow - SpawnTime).TotalSeconds > 1)
            {
                var player = nearbyEntities.FirstOrDefault(e => e is PlayerEntity && (e as PlayerEntity).Health != 0
                                                                                  && e.Position.DistanceTo(Position) <=
                                                                                  PickupRange);
                if (player != null)
                {
                    var playerEntity = player as PlayerEntity;
                    playerEntity?.OnPickUpItem(this);
                    entityManager.DespawnEntity(this);
                }
            }

            if ((DateTime.UtcNow - SpawnTime).TotalMinutes > 5)
                entityManager.DespawnEntity(this);
            base.Update(entityManager);
        }
    }
}