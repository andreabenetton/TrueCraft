using System;

namespace TrueCraft.Core.Logic.Items
{
    public class RawPorkchopItem : FoodItem
    {
        public static readonly short ItemID = 0x13F;

        public override short ID => 0x13F;

        public override float Restores => 1.5f;

        public override string DisplayName => "Raw Porkchop";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 5);
        }
    }
}