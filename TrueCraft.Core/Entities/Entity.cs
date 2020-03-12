using System;
using System.ComponentModel;
using TrueCraft.API;
using TrueCraft.API.Entities;
using TrueCraft.API.Networking;
using TrueCraft.API.Server;
using TrueCraft.API.World;

namespace TrueCraft.Core.Entities
{
    public abstract class Entity : IEntity
    {
        protected EntityFlags _EntityFlags;

        protected float _Pitch;

        protected Vector3 _Position;

        protected Vector3 _Velocity;

        protected float _Yaw;

        protected Entity()
        {
            EnablePropertyChange = true;
            EntityID = -1;
            SpawnTime = DateTime.UtcNow;
        }

        public virtual Vector3 Velocity
        {
            get => _Velocity;
            set
            {
                _Velocity = value;
                OnPropertyChanged("Velocity");
            }
        }


        public virtual EntityFlags EntityFlags
        {
            get => _EntityFlags;
            set
            {
                _EntityFlags = value;
                OnPropertyChanged("Metadata");
            }
        }

        protected bool EnablePropertyChange { get; set; }

        public DateTime SpawnTime { get; set; }

        public int EntityID { get; set; }
        public IEntityManager EntityManager { get; set; }
        public IWorld World { get; set; }

        public virtual Vector3 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                OnPropertyChanged("Position");
            }
        }

        public float Yaw
        {
            get => _Yaw;
            set
            {
                _Yaw = value;
                OnPropertyChanged("Yaw");
            }
        }

        public float Pitch
        {
            get => _Pitch;
            set
            {
                _Pitch = value;
                OnPropertyChanged("Pitch");
            }
        }

        public bool Despawned { get; set; }

        public abstract Size Size { get; }

        public abstract IPacket SpawnPacket { get; }

        public virtual bool SendMetadataToClients => false;

        public virtual MetadataDictionary Metadata
        {
            get
            {
                var dictionary = new MetadataDictionary();
                dictionary[0] = new MetadataByte((byte) EntityFlags);
                dictionary[1] = new MetadataShort(300);
                return dictionary;
            }
        }

        public virtual void Update(IEntityManager entityManager)
        {
            // TODO: Losing health and all that jazz
            if (Position.Y < -50)
                entityManager.DespawnEntity(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal virtual void OnPropertyChanged(string property)
        {
            if (!EnablePropertyChange) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}