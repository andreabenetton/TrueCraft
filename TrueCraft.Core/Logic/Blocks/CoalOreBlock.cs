using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CoalOreBlock : BlockProvider, ISmeltableItem
    {
        public static readonly byte BlockID = 0x10;

        public override byte ID => 0x10;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override string DisplayName => "Coal Ore";

        public override ToolType EffectiveTools => ToolType.Pickaxe;

        public ItemStack SmeltingOutput => new ItemStack(CoalItem.ItemID);

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 2);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(CoalItem.ItemID, 1, descriptor.Metadata)};
        }
    }
}