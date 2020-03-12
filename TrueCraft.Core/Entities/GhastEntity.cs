using TrueCraft.API;

namespace TrueCraft.Core.Entities
{
    public class GhastEntity : MobEntity
    {
        public override Size Size => new Size(4.0);

        public override short MaxHealth => 10;

        public override sbyte MobType => 56;

        public override bool Friendly => false;

        public override bool BeginUpdate()
        {
            // Ghasts can fly, no need to work out gravity
            // TODO: Think about how to deal with walls and such
            return false;
        }
    }
}