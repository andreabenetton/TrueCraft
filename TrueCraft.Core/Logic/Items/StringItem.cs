using System;

namespace TrueCraft.Core.Logic.Items
{
    public class StringItem : ItemProvider
    {
        public static readonly short ItemID = 0x11F;

        public override short ID => 0x11F;

        public override string DisplayName => "String";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 0);
        }
    }
}