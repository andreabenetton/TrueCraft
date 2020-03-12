using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public abstract class DoorBlock : BlockProvider
    {
        public abstract short ItemID { get; }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server,
            IWorld world)
        {
            var upper = ((DoorItem.DoorFlags) descriptor.Metadata & DoorItem.DoorFlags.Upper) ==
                        DoorItem.DoorFlags.Upper;
            var other = upper ? Coordinates3D.Down : Coordinates3D.Up;
            if (world.GetBlockID(descriptor.Coordinates + other) != ID)
                world.SetBlockID(descriptor.Coordinates, 0);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(ItemID)};
        }
    }

    public class WoodenDoorBlock : DoorBlock
    {
        public static readonly byte BlockID = 0x40;

        public override short ItemID => WoodenDoorItem.ItemID;

        public override byte ID => 0x40;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Wooden Door";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(1, 6);
        }

        public override void BlockLeftClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            BlockRightClicked(descriptor, face, world, user);
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            var upper = ((DoorItem.DoorFlags) descriptor.Metadata & DoorItem.DoorFlags.Upper) ==
                        DoorItem.DoorFlags.Upper;
            var other = upper ? Coordinates3D.Down : Coordinates3D.Up;
            var otherMeta = world.GetMetadata(descriptor.Coordinates + other);
            world.SetMetadata(descriptor.Coordinates, (byte) (descriptor.Metadata ^ (byte) DoorItem.DoorFlags.Open));
            world.SetMetadata(descriptor.Coordinates + other, (byte) (otherMeta ^ (byte) DoorItem.DoorFlags.Open));
            return false;
        }
    }

    public class IronDoorBlock : DoorBlock
    {
        public static readonly byte BlockID = 0x47;

        public override short ItemID => IronDoorItem.ItemID;

        public override byte ID => 0x47;

        public override double BlastResistance => 25;

        public override double Hardness => 5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Iron Door";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(1, 6);
        }
    }
}