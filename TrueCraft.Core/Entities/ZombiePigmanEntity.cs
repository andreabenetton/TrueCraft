using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class ZombiePigmanEntity : MobEntity
    {
        public override Size Size => new Size(0.6, 1.8, 0.6);

        public override short MaxHealth => 20;

        public override sbyte MobType => 57;

        public override bool Friendly => false;
    }
}