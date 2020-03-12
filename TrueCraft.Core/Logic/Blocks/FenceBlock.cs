using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FenceBlock : BlockProvider, ICraftingRecipe, IBurnableItem
    {
        public static readonly byte BlockID = 0x55;

        public override byte ID => 0x55;

        public override double BlastResistance => 15;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override bool Flammable => true;

        public override string DisplayName => "Fence";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(StickItem.ItemID),
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(StickItem.ItemID)
                    },
                    {
                        new ItemStack(StickItem.ItemID),
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(StickItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 0);
        }
    }
}