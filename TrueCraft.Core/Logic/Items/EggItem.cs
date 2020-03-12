using System;

namespace TrueCraft.Core.Logic.Items
{
    public class EggItem : ItemProvider
    {
        public static readonly short ItemID = 0x158;

        public override short ID => 0x158;

        public override sbyte MaximumStack => 16;

        public override string DisplayName => "Egg";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(12, 0);
        }
    }
}