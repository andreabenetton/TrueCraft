using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LockedChestBlock : BlockProvider, IBurnableItem
    {
        public static readonly byte BlockID = 0x5F;

        public override byte ID => 0x5F;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Locked Chest";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(10, 1);
        }
    }
}