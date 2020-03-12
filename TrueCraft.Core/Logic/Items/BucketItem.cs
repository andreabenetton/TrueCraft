using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class BucketItem : ToolItem
    {
        public static readonly short ItemID = 0x145;

        public override short ID => 0x145;

        public override string DisplayName => "Bucket";

        protected virtual byte? RelevantBlockType => null;

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 4);
        }

        public override void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (item.ID == ItemID) // Empty bucket
            {
                var block = world.GetBlockID(coordinates);
                if (block == WaterBlock.BlockID || block == StationaryWaterBlock.BlockID)
                {
                    var meta = world.GetMetadata(coordinates);
                    if (meta == 0) // Is source block?
                    {
                        user.Inventory[user.SelectedSlot] = new ItemStack(WaterBucketItem.ItemID);
                        world.SetBlockID(coordinates, 0);
                    }
                }
                else if (block == LavaBlock.BlockID || block == StationaryLavaBlock.BlockID)
                {
                    var meta = world.GetMetadata(coordinates);
                    if (meta == 0) // Is source block?
                    {
                        user.Inventory[user.SelectedSlot] = new ItemStack(LavaBucketItem.ItemID);
                        world.SetBlockID(coordinates, 0);
                    }
                }
            }
            else
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(world.GetBlockID(coordinates));
                if (!provider.Opaque)
                {
                    if (RelevantBlockType != null)
                    {
                        var blockType = RelevantBlockType.Value;
                        user.Server.BlockUpdatesEnabled = false;
                        world.SetBlockID(coordinates, blockType);
                        world.SetMetadata(coordinates, 0); // Source block
                        user.Server.BlockUpdatesEnabled = true;
                        var liquidProvider = world.BlockRepository.GetBlockProvider(blockType);
                        liquidProvider.BlockPlaced(new BlockDescriptor {Coordinates = coordinates}, face, world, user);
                    }

                    user.Inventory[user.SelectedSlot] = new ItemStack(ItemID);
                }
            }
        }
    }

    public class LavaBucketItem : BucketItem, IBurnableItem
    {
        public new static readonly short ItemID = 0x147;

        public override short ID => 0x147;

        public override string DisplayName => "Lava Bucket";

        protected override byte? RelevantBlockType => LavaBlock.BlockID;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(1000);
    }

    public class MilkItem : BucketItem
    {
        public new static readonly short ItemID = 0x14F;

        public override short ID => 0x14F;

        public override string DisplayName => "Milk";

        protected override byte? RelevantBlockType => null;
    }

    public class WaterBucketItem : BucketItem
    {
        public new static readonly short ItemID = 0x146;

        public override short ID => 0x146;

        public override string DisplayName => "Water Bucket";

        protected override byte? RelevantBlockType => WaterBlock.BlockID;
    }
}