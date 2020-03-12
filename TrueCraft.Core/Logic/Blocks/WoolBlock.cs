using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class WoolBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x23;

        public override byte ID => 0x23;

        public override double BlastResistance => 4;

        public override double Hardness => 0.8;

        public override byte Luminance => 0;

        public override string DisplayName => "Wool";

        public override bool Flammable => true;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Cloth;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(StringItem.ItemID), new ItemStack(StringItem.ItemID)},
                    {new ItemStack(StringItem.ItemID), new ItemStack(StringItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => true;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 4);
        }
    }
}