using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class ShearsItem : ToolItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x167;

        public override short ID => 0x167;

        public override sbyte MaximumStack => 1;

        public override short BaseDurability => 239;

        public override string DisplayName => "Shears";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {ItemStack.EmptyStack, new ItemStack(IronIngotItem.ItemID)},
                    {new ItemStack(IronIngotItem.ItemID), ItemStack.EmptyStack}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(13, 5);
        }
    }
}