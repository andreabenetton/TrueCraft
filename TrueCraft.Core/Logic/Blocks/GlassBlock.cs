using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class GlassBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x14;

        public override byte ID => 0x14;

        public override double BlastResistance => 1.5;

        public override double Hardness => 0.3;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Glass";

        public override byte LightOpacity => 0;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Glass;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(1, 3);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new ItemStack[0];
        }
    }
}