using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class BowItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x105;

        public override short ID => 0x105;

        public override sbyte MaximumStack => 1;

        public override string DisplayName => "Bow";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID), new ItemStack(StringItem.ItemID)},
                    {new ItemStack(StickItem.ItemID), ItemStack.EmptyStack, new ItemStack(StringItem.ItemID)},
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID), new ItemStack(StringItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 1);
        }
    }
}