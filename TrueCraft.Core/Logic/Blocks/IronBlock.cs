using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class IronBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x2A;

        public override byte ID => 0x2A;

        public override double BlastResistance => 30;

        public override double Hardness => 5;

        public override byte Luminance => 0;

        public override string DisplayName => "Block of Iron";

        public override ToolMaterial EffectiveToolMaterials =>
            ToolMaterial.Stone | ToolMaterial.Iron | ToolMaterial.Diamond;

        public override ToolType EffectiveTools => ToolType.Pickaxe;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(IronIngotItem.ItemID), new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID), new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {
                        new ItemStack(IronIngotItem.ItemID), new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 1);
        }
    }
}