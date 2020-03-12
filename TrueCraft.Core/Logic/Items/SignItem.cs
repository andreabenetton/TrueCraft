using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class SignItem : ItemProvider, ICraftingRecipe
    {
        public static readonly short ItemID = 0x143;

        public override short ID => 0x143;

        public override sbyte MaximumStack => 1;

        public override string DisplayName => "Sign";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    },
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    },
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID), ItemStack.EmptyStack}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 2);
        }

        public override void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            if (face == BlockFace.PositiveY)
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(UprightSignBlock.BlockID);
                provider.ItemUsedOnBlock(coordinates, item, face, world, user);
            }
            else
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(WallSignBlock.BlockID);
                provider.ItemUsedOnBlock(coordinates, item, face, world, user);
            }
        }
    }
}