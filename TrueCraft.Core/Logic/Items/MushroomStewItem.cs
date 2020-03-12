using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class MushroomStewItem : FoodItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x11A;

        public override short ID => 0x11A;

        public override sbyte MaximumStack => 1;

        public override float Restores => 5;

        public override string DisplayName => "Mushroom Stew";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(RedMushroomBlock.BlockID)},
                    {new ItemStack(BrownMushroomBlock.BlockID)},
                    {new ItemStack(BowlItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 4);
        }
    }
}