using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class DirtBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x03;

        public override byte ID => 0x03;

        public override double BlastResistance => 2.5;

        public override double Hardness => 0.5;

        public override byte Luminance => 0;

        public override string DisplayName => "Dirt";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Gravel;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 0);
        }
    }
}