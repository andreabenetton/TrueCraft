using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class PistonBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x21;

        public override byte ID => 0x21;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Piston";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID),
                        new ItemStack(WoodenPlanksBlock.BlockID)
                    },
                    {
                        new ItemStack(CobblestoneBlock.BlockID),
                        new ItemStack(IronIngotItem.ItemID),
                        new ItemStack(CobblestoneBlock.BlockID)
                    },
                    {
                        new ItemStack(CobblestoneBlock.BlockID),
                        new ItemStack(RedstoneItem.ItemID),
                        new ItemStack(CobblestoneBlock.BlockID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(11, 6);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            world.SetMetadata(descriptor.Coordinates,
                (byte) MathHelper.DirectionByRotation(user.Entity.Position, user.Entity.Yaw,
                    descriptor.Coordinates, true));
        }
    }

    public class StickyPistonBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x1D;

        public override byte ID => 0x1D;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Sticky Piston";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(SlimeballItem.ItemID)},
                    {new ItemStack(PistonBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(10, 6);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            world.SetMetadata(descriptor.Coordinates,
                (byte) MathHelper.DirectionByRotation(user.Entity.Position, user.Entity.Yaw,
                    descriptor.Coordinates, true));
        }
    }

    public class PistonPlungerBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x22;

        public override byte ID => 0x22;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override string DisplayName => "Piston Plunger";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(11, 6);
        }
    }

    public class PistonPlaceholderBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x24;

        public override byte ID => 0x24;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override string DisplayName => "Piston Placeholder";
    }
}