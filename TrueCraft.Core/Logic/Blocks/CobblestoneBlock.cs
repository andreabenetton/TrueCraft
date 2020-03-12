using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CobblestoneBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x04;

        public override byte ID => 0x04;

        public override double BlastResistance => 30;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override string DisplayName => "Cobblestone";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 1);
        }
    }
}