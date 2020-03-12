using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class JackoLanternBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x5B;

        public override byte ID => 0x5B;

        public override double BlastResistance => 5;

        public override double Hardness => 1;

        public override byte Luminance => 15;

        public override bool Opaque => false;

        public override byte LightOpacity => 255;

        public override string DisplayName => "Jack 'o' Lantern";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(PumpkinBlock.BlockID)},
                    {new ItemStack(TorchBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

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