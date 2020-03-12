using System;

namespace TrueCraft.Core.Logic.Items
{
    public class MusicDiscItem : ItemProvider
    {
        public static readonly short ItemID = 0x8D1;

        public override short ID => 0x8D1;

        public override sbyte MaximumStack => 1;

        public override string DisplayName => "Music Disc";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 15);
        }
    }
}