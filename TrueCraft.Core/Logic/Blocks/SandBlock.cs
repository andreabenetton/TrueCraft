using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.Core.Entities;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SandBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x0C;

        public override byte ID => 0x0C;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override string DisplayName => "Sand";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Sand;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 1);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            BlockUpdate(descriptor, descriptor, user.Server, world);
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server,
            IWorld world)
        {
            if (world.GetBlockID(descriptor.Coordinates + Coordinates3D.Down) == AirBlock.BlockID)
            {
                world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
                server.GetEntityManagerForWorld(world).SpawnEntity(new FallingSandEntity(descriptor.Coordinates));
            }
        }
    }
}