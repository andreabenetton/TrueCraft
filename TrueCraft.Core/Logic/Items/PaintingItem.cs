using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class PaintingItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x141;

        public override short ID => 0x141;

        public override string DisplayName => "Painting";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StickItem.ItemID), new ItemStack(StickItem.ItemID), new ItemStack(StickItem.ItemID)},
                    {
                        new ItemStack(StickItem.ItemID), new ItemStack(WoolBlock.BlockID),
                        new ItemStack(StickItem.ItemID)
                    },
                    {new ItemStack(StickItem.ItemID), new ItemStack(StickItem.ItemID), new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 1);
        }
    }
}