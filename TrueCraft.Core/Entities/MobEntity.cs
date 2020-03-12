using System;
using TrueCraft.API;
using TrueCraft.API.AI;
using TrueCraft.API.Entities;
using TrueCraft.API.Networking;
using TrueCraft.API.Server;
using TrueCraft.Core.AI;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Entities
{
    public abstract class MobEntity : LivingEntity, IMobEntity
    {
        protected MobEntity()
        {
            CurrentState = new WanderState();
        }

        public abstract sbyte MobType { get; }

        public virtual bool Friendly => true;

        /// <summary>
        ///     Mob's current speed in m/s.
        /// </summary>
        public virtual double Speed { get; set; } = 4;

        public event EventHandler PathComplete;

        public override IPacket SpawnPacket =>
            new SpawnMobPacket(EntityID, MobType,
                MathHelper.CreateAbsoluteInt(Position.X),
                MathHelper.CreateAbsoluteInt(Position.Y),
                MathHelper.CreateAbsoluteInt(Position.Z),
                MathHelper.CreateRotationByte(Yaw),
                MathHelper.CreateRotationByte(Pitch),
                Metadata);

        public virtual void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            // This space intentionally left blank
        }

        public BoundingBox BoundingBox => new BoundingBox(Position, Position + Size);

        public virtual bool BeginUpdate()
        {
            EnablePropertyChange = false;
            return true;
        }

        public virtual void EndUpdate(Vector3 newPosition)
        {
            EnablePropertyChange = true;
            Position = newPosition;
        }

        public float AccelerationDueToGravity => 1.6f;

        public float Drag => 0.40f;

        public float TerminalVelocity => 78.4f;

        public PathResult CurrentPath { get; set; }

        public IMobState CurrentState { get; set; }

        public void Face(Vector3 target)
        {
            var diff = target - Position;
            Yaw = (float) MathHelper.RadiansToDegrees(-(Math.Atan2(diff.X, diff.Z) - Math.PI) +
                                                      Math.PI); // "Flip" over the 180 mark
        }

        public bool AdvancePath(TimeSpan time, bool faceRoute = true)
        {
            var modifier = time.TotalSeconds * Speed;
            if (CurrentPath != null)
            {
                // Advance along path
                var target = (Vector3) CurrentPath.Waypoints[CurrentPath.Index];
                target += new Vector3(Size.Width / 2, 0, Size.Depth / 2); // Center it
                target.Y = Position.Y; // TODO: Find better way of doing this
                if (faceRoute)
                    Face(target);
                var lookAt =
                    Vector3.Forwards.Transform(Matrix.CreateRotationY(MathHelper.ToRadians(-(Yaw - 180) + 180)));
                lookAt *= modifier;
                Velocity = new Vector3(lookAt.X, Velocity.Y, lookAt.Z);
                if (Position.DistanceTo(target) < Velocity.Distance)
                {
                    Position = target;
                    Velocity = Vector3.Zero;
                    CurrentPath.Index++;
                    if (CurrentPath.Index >= CurrentPath.Waypoints.Count)
                    {
                        CurrentPath = null;
                        PathComplete?.Invoke(this, null);
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Update(IEntityManager entityManager)
        {
            if (CurrentState != null)
                CurrentState.Update(this, entityManager);
            else
                AdvancePath(entityManager.TimeSinceLastUpdate);
            base.Update(entityManager);
        }
    }
}