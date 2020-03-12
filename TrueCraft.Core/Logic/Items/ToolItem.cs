using TrueCraft.API;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ToolItem : ItemProvider
    {
        public virtual ToolMaterial Material => ToolMaterial.None;

        public virtual ToolType ToolType => ToolType.None;

        public virtual short BaseDurability => 0;

        public override sbyte MaximumStack => 1;

        public virtual int Uses => BaseDurability;
    }
}