using System;

namespace TrueCraft.Core.Logic.Items
{
    public class AppleItem : FoodItem
    {
        public static readonly short ItemID = 0x104;

        public override short ID => 0x104;

        public override float Restores => 2;

        public override string DisplayName => "Apple";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 0);
        }
    }
}