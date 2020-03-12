using System;

namespace TrueCraft.Core.Logic.Items
{
    public class FeatherItem : ItemProvider
    {
        public static readonly short ItemID = 0x120;

        public override short ID => 0x120;

        public override string DisplayName => "Feather";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 1);
        }
    }
}