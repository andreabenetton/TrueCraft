using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class WoodenPlanksBlock : BlockProvider, ICraftingRecipe, IBurnableItem
    {
        public static readonly byte BlockID = 0x05;

        public override byte ID => 0x05;

        public override double BlastResistance => 15;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override string DisplayName => "Wooden Planks";

        public override bool Flammable => true;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(WoodBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID, 4);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 0);
        }
    }
}