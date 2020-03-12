using System;

namespace TrueCraft.Core.Logic.Items
{
    public class BrickItem : ItemProvider
    {
        public static readonly short ItemID = 0x150;

        public override short ID => 0x150;

        public override string DisplayName => "Brick";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(6, 1);
        }
    }
}