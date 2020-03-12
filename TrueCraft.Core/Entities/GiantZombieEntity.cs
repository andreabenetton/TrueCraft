using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class GiantZombieEntity : ZombieEntity
    {
        public override Size Size => new Size(0.6, 1.8, 0.6) * 6;

        public override short MaxHealth => 100;

        public override sbyte MobType => 53;

        public override bool Friendly => false;
    }
}