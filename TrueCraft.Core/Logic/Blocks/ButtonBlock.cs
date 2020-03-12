using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class ButtonBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x4D;

        public override byte ID => 0x4D;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Button";

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StoneBlock.BlockID)},
                    {new ItemStack(StoneBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;
    }
}