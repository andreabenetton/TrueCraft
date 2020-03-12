using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CakeBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x5C;

        public override byte ID => 0x5C;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Cake";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Cloth;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(MilkItem.ItemID),
                        new ItemStack(MilkItem.ItemID),
                        new ItemStack(MilkItem.ItemID)
                    },
                    {
                        new ItemStack(SugarItem.ItemID),
                        new ItemStack(EggItem.ItemID),
                        new ItemStack(SugarItem.ItemID)
                    },
                    {
                        new ItemStack(WheatItem.ItemID),
                        new ItemStack(WheatItem.ItemID),
                        new ItemStack(WheatItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(9, 7);
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            if (descriptor.Metadata == 5)
                world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
            else
                world.SetMetadata(descriptor.Coordinates, (byte) (descriptor.Metadata + 1));
            return false;
        }
    }
}