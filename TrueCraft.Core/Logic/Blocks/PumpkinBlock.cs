using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class PumpkinBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x56;

        public override byte ID => 0x56;

        public override double BlastResistance => 5;

        public override double Hardness => 1;

        public override byte Luminance => 0;

        public override string DisplayName => "Pumpkin";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 6);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            world.SetMetadata(descriptor.Coordinates, (byte) MathHelper.DirectionByRotationFlat(user.Entity.Yaw, true));
        }
    }
}