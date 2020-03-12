using System;

namespace TrueCraft.Core.Logic.Items
{
    public class WheatItem : ItemProvider
    {
        public static readonly short ItemID = 0x128;

        public override short ID => 0x128;

        public override string DisplayName => "Wheat";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(9, 1);
        }
    }
}