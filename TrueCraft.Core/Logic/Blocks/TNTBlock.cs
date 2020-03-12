using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class TNTBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x2E;

        public override byte ID => 0x2E;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override string DisplayName => "TNT";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Grass;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(GunpowderItem.ItemID),
                        new ItemStack(SandBlock.BlockID),
                        new ItemStack(GunpowderItem.ItemID)
                    },
                    {
                        new ItemStack(SandBlock.BlockID),
                        new ItemStack(GunpowderItem.ItemID),
                        new ItemStack(SandBlock.BlockID)
                    },
                    {
                        new ItemStack(GunpowderItem.ItemID),
                        new ItemStack(SandBlock.BlockID),
                        new ItemStack(GunpowderItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(8, 0);
        }
    }
}