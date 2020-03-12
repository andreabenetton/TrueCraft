using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SpongeBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x13;

        public override byte ID => 0x13;

        public override double BlastResistance => 3;

        public override double Hardness => 0.6;

        public override byte Luminance => 0;

        public override string DisplayName => "Sponge";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Grass;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 3);
        }
    }
}