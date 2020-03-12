using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class SquidEntity : MobEntity
    {
        public override Size Size => new Size(0.95);

        public override short MaxHealth => 10;

        public override sbyte MobType => 94;
    }
}