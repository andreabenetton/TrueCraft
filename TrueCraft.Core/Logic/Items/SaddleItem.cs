using System;

namespace TrueCraft.Core.Logic.Items
{
    public class SaddleItem : ItemProvider
    {
        public static readonly short ItemID = 0x149;

        public override short ID => 0x149;

        public override sbyte MaximumStack => 1;

        public override string DisplayName => "Saddle";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 6);
        }
    }
}