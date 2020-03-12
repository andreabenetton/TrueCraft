using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class AirBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x00;

        public override byte ID => 0x00;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override bool Opaque => false;

        public override byte Luminance => 0;

        public override string DisplayName => "Air";

        public override BoundingBox? BoundingBox => null;

        public override SoundEffectClass SoundEffect => SoundEffectClass.None;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 0);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new ItemStack[0];
        }
    }
}