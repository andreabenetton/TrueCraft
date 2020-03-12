using System;

namespace TrueCraft.Core.Logic.Items
{
    public class DiamondItem : ItemProvider
    {
        public static readonly short ItemID = 0x108;

        public override short ID => 0x108;

        public override string DisplayName => "Diamond";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 3);
        }
    }
}