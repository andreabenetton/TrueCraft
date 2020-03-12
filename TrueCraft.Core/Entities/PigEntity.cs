using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class PigEntity : MobEntity
    {
        public override Size Size => new Size(0.9);
        public override short MaxHealth => 10;
        public override sbyte MobType => 90;
    }
}