using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class NetherrackBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x57;

        public override byte ID => 0x57;

        public override double BlastResistance => 2;

        public override double Hardness => 0.4;

        public override byte Luminance => 0;

        public override string DisplayName => "Netherrack";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(7, 6);
        }
    }
}