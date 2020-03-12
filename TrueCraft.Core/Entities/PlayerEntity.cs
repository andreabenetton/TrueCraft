using System;
using TrueCraft.API;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Entities
{
    public class PlayerEntity : LivingEntity
    {
        public const double Width = 0.6;
        public const double Height = 1.62;
        public const double Depth = 0.6;

        protected short _SelectedSlot;

        protected Vector3 _SpawnPoint;

        public PlayerEntity(string username)
        {
            Username = username;
        }

        public override IPacket SpawnPacket =>
            new SpawnPlayerPacket(EntityID, Username,
                MathHelper.CreateAbsoluteInt(Position.X),
                MathHelper.CreateAbsoluteInt(Position.Y),
                MathHelper.CreateAbsoluteInt(Position.Z),
                MathHelper.CreateRotationByte(Yaw),
                MathHelper.CreateRotationByte(Pitch), 0 /* Note: current item is set through other means */);

        public override Size Size => new Size(Width, Height, Depth);

        public override short MaxHealth => 20;

        public string Username { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        public double PositiveDeltaY { get; set; }

        public Vector3 OldPosition { get; private set; }

        public override Vector3 Position
        {
            get => _Position;
            set
            {
                OldPosition = _Position;
                _Position = value;
                OnPropertyChanged("Position");
            }
        }

        public short SelectedSlot
        {
            get => _SelectedSlot;
            set
            {
                _SelectedSlot = value;
                OnPropertyChanged("SelectedSlot");
            }
        }

        public ItemStack ItemInMouse { get; set; }

        public Vector3 SpawnPoint
        {
            get => _SpawnPoint;
            set
            {
                _SpawnPoint = value;
                OnPropertyChanged("SpawnPoint");
            }
        }

        public event EventHandler<EntityEventArgs> PickUpItem;

        public void OnPickUpItem(ItemEntity item)
        {
            PickUpItem?.Invoke(this, new EntityEventArgs(item));
        }
    }
}