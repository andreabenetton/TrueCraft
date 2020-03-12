using System;

namespace TrueCraft.Core.Logic.Items
{
    public class RawFishItem : FoodItem
    {
        public static readonly short ItemID = 0x15D;

        public override short ID => 0x15D;

        public override float Restores => 1;

        public override string DisplayName => "Raw Fish";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(9, 5);
        }
    }
}