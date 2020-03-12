using System;

namespace TrueCraft.Core.Logic.Items
{
    public class FlintItem : ItemProvider
    {
        public static readonly short ItemID = 0x13E;

        public override short ID => 0x13E;

        public override string DisplayName => "Flint";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }
    }
}