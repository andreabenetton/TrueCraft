using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LeverBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x45;

        public override byte ID => 0x45;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Lever";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StickItem.ItemID)},
                    {new ItemStack(CobblestoneBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 6);
        }
    }
}