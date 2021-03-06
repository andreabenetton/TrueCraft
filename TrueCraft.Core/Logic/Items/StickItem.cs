using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class StickItem : ItemProvider, ICraftingRecipe, IBurnableItem
    {
        public static readonly short ItemID = 0x118;

        public override short ID => 0x118;

        public override string DisplayName => "Stick";

        public TimeSpan BurnTime => TimeSpan.FromSeconds(5);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(WoodenPlanksBlock.BlockID)},
                    {new ItemStack(WoodenPlanksBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID, 4);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 3);
        }
    }
}