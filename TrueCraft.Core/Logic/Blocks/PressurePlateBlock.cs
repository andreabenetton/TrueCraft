using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public abstract class PressurePlateBlock : BlockProvider
    {
        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;
    }

    public class WoodenPressurePlateBlock : PressurePlateBlock, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x48;

        public override byte ID => 0x48;

        public override string DisplayName => "Wooden Pressure Plate";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;
    }

    public class StonePressurePlateBlock : PressurePlateBlock, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x46;

        public override byte ID => 0x46;

        public override string DisplayName => "Stone Pressure Plate";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StoneBlock.BlockID), new ItemStack(StoneBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;
    }
}