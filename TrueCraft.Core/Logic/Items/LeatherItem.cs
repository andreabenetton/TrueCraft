using System;

namespace TrueCraft.Core.Logic.Items
{
    public class LeatherItem : ItemProvider
    {
        public static readonly short ItemID = 0x14E;

        public override short ID => 0x14E;

        public override string DisplayName => "Leather";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 6);
        }
    }
}