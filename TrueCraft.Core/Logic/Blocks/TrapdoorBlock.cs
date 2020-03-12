using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class TrapdoorBlock : BlockProvider, ICraftingRecipe, IBurnableItem
    {
        public enum TrapdoorDirection
        {
            West = 0x0,
            East = 0x1,
            South = 0x2,
            North = 0x3
        }

        [Flags]
        public enum TrapdoorFlags
        {
            Closed = 0x0,
            Open = 0x4
        }

        public static readonly byte BlockID = 0x60;

        public override byte ID => 0x60;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Trapdoor";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

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
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 5);
        }

        public override void BlockLeftClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            BlockRightClicked(descriptor, face, world, user);
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            // Flip bit back and forth between Open and Closed
            world.SetMetadata(descriptor.Coordinates, (byte) (descriptor.Metadata ^ (byte) TrapdoorFlags.Open));
            return false;
        }

        public override void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            if (face == BlockFace.PositiveY || face == BlockFace.NegativeY)
                // Trapdoors are not placed when the user clicks on the top or bottom of a block
                return;

            // NOTE: These directions are rotated by 90 degrees so that the hinge of the trapdoor is placed
            // where the user had their cursor.
            switch (face)
            {
                case BlockFace.NegativeZ:
                    item.Metadata = (byte) TrapdoorDirection.West;
                    break;
                case BlockFace.PositiveZ:
                    item.Metadata = (byte) TrapdoorDirection.East;
                    break;
                case BlockFace.NegativeX:
                    item.Metadata = (byte) TrapdoorDirection.South;
                    break;
                case BlockFace.PositiveX:
                    item.Metadata = (byte) TrapdoorDirection.North;
                    break;
                default:
                    return;
            }

            base.ItemUsedOnBlock(coordinates, item, face, world, user);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(ID)};
        }
    }
}