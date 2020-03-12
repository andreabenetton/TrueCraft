using System;

namespace TrueCraft.Core.Logic.Items
{
    public class GunpowderItem : ItemProvider
    {
        public static readonly short ItemID = 0x121;

        public override short ID => 0x121;

        public override string DisplayName => "Gunpowder";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 2);
        }
    }
}