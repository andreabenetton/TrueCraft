using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class MinecartItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x148;

        public override short ID => 0x148;

        public override sbyte MaximumStack => 1;

        public override string DisplayName => "Minecart";

        public virtual ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(IronIngotItem.ItemID), ItemStack.EmptyStack, new ItemStack(IronIngotItem.ItemID)},
                    {
                        new ItemStack(IronIngotItem.ItemID), new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(IronIngotItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 8);
        }
    }

    public class MinecartWithChestItem : MinecartItem, ICraftingRecipe
    {
        public new static readonly short ItemID = 0x156;

        public override short ID => 0x156;

        public override string DisplayName => "Minecart with Chest";

        public override ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(ChestBlock.BlockID)},
                    {new ItemStack(MinecartItem.ItemID)}
                };
            }
        }
    }

    public class MinecartWithFurnaceItem : MinecartItem, ICraftingRecipe
    {
        public new static readonly short ItemID = 0x157;

        public override short ID => 0x157;

        public override string DisplayName => "Minecart with Furnace";

        public override ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(FurnaceBlock.BlockID)},
                    {new ItemStack(MinecartItem.ItemID)}
                };
            }
        }
    }
}