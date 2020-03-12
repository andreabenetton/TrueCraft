using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class WoodBlock : BlockProvider, IBurnableItem
    {
        public enum WoodType
        {
            Oak = 0,
            Spruce = 1,
            Birch = 2
        }

        public static readonly byte BlockID = 0x11;

        public override byte ID => 0x11;

        public override double BlastResistance => 10;

        public override double Hardness => 2;

        public override byte Luminance => 0;

        public override string DisplayName => "Wood";

        public override bool Flammable => true;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 1);
        }
    }
}