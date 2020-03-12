using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class CompassItem : ToolItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x159;

        public override short ID => 0x159;

        public override string DisplayName => "Compass";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {ItemStack.EmptyStack, new ItemStack(IronIngotItem.ItemID), ItemStack.EmptyStack},
                    {
                        new ItemStack(IronIngotItem.ItemID), new ItemStack(RedstoneItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    },
                    {ItemStack.EmptyStack, new ItemStack(IronIngotItem.ItemID), ItemStack.EmptyStack}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(6, 3);
        }
    }
}