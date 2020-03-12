using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class ClayBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x52;

        public override byte ID => 0x52;

        public override double BlastResistance => 3;

        public override double Hardness => 0.6;

        public override byte Luminance => 0;

        public override string DisplayName => "Clay";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Gravel;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(ClayItem.ItemID), new ItemStack(ClayItem.ItemID)},
                    {new ItemStack(ClayItem.ItemID), new ItemStack(ClayItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(8, 4);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(ClayItem.ItemID, 4)};
        }
    }
}