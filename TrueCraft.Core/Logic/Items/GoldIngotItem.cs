using System;

namespace TrueCraft.Core.Logic.Items
{
    public class GoldIngotItem : ItemProvider
    {
        public static readonly short ItemID = 0x10A;

        public override short ID => 0x10A;

        public override string DisplayName => "Gold Ingot";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 2);
        }
    }
}