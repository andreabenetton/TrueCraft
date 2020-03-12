namespace TrueCraft.Core.Entities
{
    public abstract class LivingEntity : Entity
    {
        protected short _Air;

        protected float _HeadYaw;

        protected short _Health;

        protected LivingEntity()
        {
            Health = MaxHealth;
        }

        public short Air
        {
            get => _Air;
            set
            {
                _Air = value;
                OnPropertyChanged("Air");
            }
        }

        public short Health
        {
            get => _Health;
            set
            {
                _Health = value;
                OnPropertyChanged("Health");
            }
        }

        public float HeadYaw
        {
            get => _HeadYaw;
            set
            {
                _HeadYaw = value;
                OnPropertyChanged("HeadYaw");
            }
        }

        public override bool SendMetadataToClients => true;

        public abstract short MaxHealth { get; }
    }
}