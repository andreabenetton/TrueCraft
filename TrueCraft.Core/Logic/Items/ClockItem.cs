using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class ClockItem : ToolItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x15B;

        public override short ID => 0x15B;

        public override string DisplayName => "Clock";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {ItemStack.EmptyStack, new ItemStack(GoldIngotItem.ItemID), ItemStack.EmptyStack},
                    {
                        new ItemStack(GoldIngotItem.ItemID), new ItemStack(RedstoneItem.ItemID),
                        new ItemStack(GoldIngotItem.ItemID)
                    },
                    {ItemStack.EmptyStack, new ItemStack(GoldIngotItem.ItemID), ItemStack.EmptyStack}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(6, 4);
        }
    }
}