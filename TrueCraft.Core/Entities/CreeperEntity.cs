using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class CreeperEntity : MobEntity
    {
        public override Size Size => new Size(0.6, 1.8, 0.6);
        public override short MaxHealth => 20;
        public override sbyte MobType => 50;
        public override bool Friendly => false;
    }
}