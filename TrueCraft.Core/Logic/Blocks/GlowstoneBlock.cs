using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class GlowstoneBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x59;

        public override byte ID => 0x59;

        public override double BlastResistance => 1.5;

        public override double Hardness => 0.3;

        public override byte Luminance => 15;

        public override bool Opaque => false;

        public override string DisplayName => "Glowstone";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Glass;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {
                        new ItemStack(GlowstoneDustItem.ItemID),
                        new ItemStack(GlowstoneDustItem.ItemID)
                    },
                    {
                        new ItemStack(GlowstoneDustItem.ItemID),
                        new ItemStack(GlowstoneDustItem.ItemID)
                    }
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(9, 6);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[]
                {new ItemStack(GlowstoneDustItem.ItemID, (sbyte) new Random().Next(2, 4), descriptor.Metadata)};
        }
    }
}