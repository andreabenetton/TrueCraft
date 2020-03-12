using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class BookshelfBlock : BlockProvider, ICraftingRecipe, IBurnableItem
    {
        public static readonly byte BlockID = 0x2F;

        public override byte ID => 0x2F;

        public override double BlastResistance => 7.5;

        public override double Hardness => 1.5;

        public override byte Luminance => 0;

        public override string DisplayName => "Bookshelf";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override bool Flammable => true;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    },
                    {
                        new ItemStack(BookItem.ItemID),
                        new ItemStack(BookItem.ItemID),
                        new ItemStack(BookItem.ItemID)
                    },
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 2);
        }
    }
}