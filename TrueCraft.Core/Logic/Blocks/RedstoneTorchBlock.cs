using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class RedstoneTorchBlock : TorchBlock, ICraftingRecipe
    {
        public new static readonly byte BlockID = 0x4C;

        public override byte ID => 0x4C;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 7;

        public override bool Opaque => false;

        public override string DisplayName => "Redstone Torch";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(RedstoneDustBlock.BlockID)},
                    {new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public override ItemStack Output => new ItemStack(BlockID);

        public override bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 6);
        }
    }

    public class InactiveRedstoneTorchBlock : RedstoneTorchBlock
    {
        public new static readonly byte BlockID = 0x4B;

        public override byte ID => 0x4B;

        public override byte Luminance => 0;

        public override string DisplayName => "Redstone Torch (inactive)";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 7);
        }
    }
}