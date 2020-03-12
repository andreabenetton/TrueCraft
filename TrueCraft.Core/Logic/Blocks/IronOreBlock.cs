using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class IronOreBlock : BlockProvider, ISmeltableItem
    {
        public static readonly byte BlockID = 0x0F;

        public override byte ID => 0x0F;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override string DisplayName => "Iron Ore";

        public override ToolMaterial EffectiveToolMaterials =>
            ToolMaterial.Stone | ToolMaterial.Iron | ToolMaterial.Diamond;

        public override ToolType EffectiveTools => ToolType.Pickaxe;

        public ItemStack SmeltingOutput => new ItemStack(IronIngotItem.ItemID);

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(1, 2);
        }
    }
}