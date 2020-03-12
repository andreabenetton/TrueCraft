using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public abstract class StairsBlock : BlockProvider
    {
        public enum StairDirection
        {
            East = 0,
            West = 1,
            South = 2,
            North = 3
        }

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override byte LightOpacity => 255;

        public virtual bool SignificantMetadata => false;

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            byte meta = 0;
            switch (MathHelper.DirectionByRotationFlat(user.Entity.Yaw))
            {
                case Direction.East:
                    meta = (byte) StairDirection.East;
                    break;
                case Direction.West:
                    meta = (byte) StairDirection.West;
                    break;
                case Direction.North:
                    meta = (byte) StairDirection.North;
                    break;
                case Direction.South:
                    meta = (byte) StairDirection.South;
                    break;
                default:
                    meta = 0; // Should never happen
                    break;
            }

            world.SetMetadata(descriptor.Coordinates, meta);
        }
    }

    public class WoodenStairsBlock : StairsBlock, ICraftingRecipe, IBurnableItem
    {
        public static readonly byte BlockID = 0x35;

        public override byte ID => 0x35;

        public override double BlastResistance => 15;

        public override string DisplayName => "Wooden Stairs";

        public override bool Flammable => true;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(WoodenPlanksBlock.BlockID), ItemStack.EmptyStack, ItemStack.EmptyStack},
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID),
                        ItemStack.EmptyStack
                    },
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);
    }

    public class StoneStairsBlock : StairsBlock, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x43;

        public override byte ID => 0x43;

        public override double BlastResistance => 30;

        public override string DisplayName => "Stone Stairs";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StoneBlock.BlockID), ItemStack.EmptyStack, ItemStack.EmptyStack},
                    {new ItemStack(StoneBlock.BlockID), new ItemStack(StoneBlock.BlockID), ItemStack.EmptyStack},
                    {
                        new ItemStack(StoneBlock.BlockID), new ItemStack(StoneBlock.BlockID),
                        new ItemStack(StoneBlock.BlockID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);
    }
}