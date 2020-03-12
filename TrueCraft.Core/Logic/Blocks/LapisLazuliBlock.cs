using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LapisLazuliBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x16;

        public override byte ID => 0x16;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override string DisplayName => "Lapis Lazuli Block";

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
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli)
                    },
                    {
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli)
                    },
                    {
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.LapisLazuli)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 9);
        }
    }
}