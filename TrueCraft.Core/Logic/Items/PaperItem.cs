using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class PaperItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x153;

        public override short ID => 0x153;

        public override string DisplayName => "Paper";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(SugarCanesItem.ItemID), new ItemStack(SugarCanesItem.ItemID),
                        new ItemStack(SugarCanesItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 3);
        }
    }
}