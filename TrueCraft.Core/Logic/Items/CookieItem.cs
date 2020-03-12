using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class CookieItem : FoodItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x165;

        public override short ID => 0x165;

        public override sbyte MaximumStack => 8;

        public override float Restores => 0.5f;

        public override string DisplayName => "Cookie";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(WheatItem.ItemID),
                        new ItemStack(DyeItem.ItemID, 1, (short) DyeItem.DyeType.CocoaBeans),
                        new ItemStack(WheatItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(12, 5);
        }
    }
}