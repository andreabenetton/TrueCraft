using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class FlintAndSteelItem : ToolItem, ICraftingRecipe
    {
        public static readonly short ItemID = 0x103;
        public override short ID => 0x103;
        public override sbyte MaximumStack => 1;
        public override short BaseDurability => 65;
        public override string DisplayName => "Flint and Steel";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(IronIngotItem.ItemID), new ItemStack(FlintItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ItemID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 0);
        }

        public override void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (world.GetBlockID(coordinates) == AirBlock.BlockID)
            {
                world.SetBlockID(coordinates, FireBlock.BlockID);
                world.BlockRepository.GetBlockProvider(FireBlock.BlockID)
                    .BlockPlaced(world.GetBlockData(coordinates), face, world, user);

                var slot = user.SelectedItem;
                slot.Metadata += 1;
                if (slot.Metadata >= Uses)
                    slot.Count = 0; // Destroy item
                user.Inventory[user.SelectedSlot] = slot;
            }
        }
    }
}