using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SandstoneBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x18;

        public override byte ID => 0x18;

        public override double BlastResistance => 4;

        public override double Hardness => 0.8;

        public override byte Luminance => 0;

        public override string DisplayName => "Sandstone";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 12);
        }
    }
}