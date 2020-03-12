using TrueCraft.API;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ArmorItem : ItemProvider
    {
        public abstract ArmorMaterial Material { get; }

        public virtual short BaseDurability => 0;

        public abstract float BaseArmor { get; }

        public override sbyte MaximumStack => 1;
    }
}