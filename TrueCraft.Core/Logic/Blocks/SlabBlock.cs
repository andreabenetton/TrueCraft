using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SlabBlock : BlockProvider
    {
        public enum SlabMaterial
        {
            Stone = 0x0,
            Standstone = 0x1,
            Wooden = 0x2,
            Cobblestone = 0x3
        }

        public static readonly byte BlockID = 0x2C;

        public override byte ID => 0x2C;

        public override double BlastResistance => 30;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override byte LightOpacity => 255;

        public override string DisplayName => "Stone Slab";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood; // TODO: Deal with metadata god dammit

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }

        public class StoneSlabRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(StoneBlock.BlockID), new ItemStack(StoneBlock.BlockID),
                            new ItemStack(StoneBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(BlockID, 3, (short) SlabMaterial.Stone);

            public bool SignificantMetadata => true;
        }

        public class StandstoneSlabRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(SandstoneBlock.BlockID), new ItemStack(SandstoneBlock.BlockID),
                            new ItemStack(SandstoneBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(BlockID, 3, (short) SlabMaterial.Standstone);

            public bool SignificantMetadata => true;
        }

        public class WoodenSlabRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID),
                            new ItemStack(WoodenPlanksBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(BlockID, 3, (short) SlabMaterial.Wooden);

            public bool SignificantMetadata => true;
        }

        public class CobblestoneSlabRecipe : ICraftingRecipe
        {
            public ItemStack[,] Pattern
            {
                get
                {
                    return new[,]
                    {
                        {
                            new ItemStack(CobblestoneBlock.BlockID), new ItemStack(CobblestoneBlock.BlockID),
                            new ItemStack(CobblestoneBlock.BlockID)
                        }
                    };
                }
            }

            public ItemStack Output => new ItemStack(BlockID, 3, (short) SlabMaterial.Cobblestone);

            public bool SignificantMetadata => true;
        }
    }

    public class DoubleSlabBlock : SlabBlock
    {
        public new static readonly byte BlockID = 0x2B;

        public override byte ID => 0x2B;

        public override double BlastResistance => 30;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override string DisplayName => "Double Stone Slab";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }
    }
}