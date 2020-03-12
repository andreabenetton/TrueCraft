using System;

namespace TrueCraft.Core.Logic.Items
{
    public class SlimeballItem : ItemProvider
    {
        public static readonly short ItemID = 0x155;

        public override short ID => 0x155;

        public override string DisplayName => "Slimeball";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(14, 1);
        }
    }
}