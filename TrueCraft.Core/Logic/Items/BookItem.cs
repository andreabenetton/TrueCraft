using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class BookItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x154;

        public override short ID => 0x154;

        public override sbyte MaximumStack => 64;

        public override string DisplayName => "Book";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(PaperItem.ItemID)},
                    {new ItemStack(PaperItem.ItemID)},
                    {new ItemStack(PaperItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(11, 3);
        }
    }
}