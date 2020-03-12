using System;

namespace TrueCraft.Core.Logic.Items
{
    public class SnowballItem : ItemProvider
    {
        public static readonly short ItemID = 0x14C;

        public override short ID => 0x14C;

        public override sbyte MaximumStack => 16;

        public override string DisplayName => "Snowball";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(14, 0);
        }
    }
}