using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class RedstoneRepeaterBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x5D;

        public override byte ID => 0x5D;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Redstone Repeater";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }
    }

    public class ActiveRedstoneRepeaterBlock : RedstoneRepeaterBlock
    {
        public new static readonly byte BlockID = 0x5E;

        public override byte ID => 0x5E;

        public override string DisplayName => "Redstone Repeater (active)";
    }
}