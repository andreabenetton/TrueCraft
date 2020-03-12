using System;

namespace TrueCraft.Core.Logic.Items
{
    public class CookedPorkchopItem : FoodItem
    {
        public static readonly short ItemID = 0x140;

        public override short ID => 0x140;

        public override float Restores => 4;

        public override string DisplayName => "Cooked Porkchop";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 5);
        }
    }
}