using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class ArrowItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x106;

        public override short ID => 0x106;

        public override string DisplayName => "Arrow";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(FlintItem.ItemID)},
                    {new ItemStack(StickItem.ItemID)},
                    {new ItemStack(FeatherItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 2);
        }
    }
}