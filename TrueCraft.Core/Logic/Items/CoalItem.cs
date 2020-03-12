using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public class CoalItem : ItemProvider, IBurnableItem
    {
        public static readonly short ItemID = 0x107;

        public override short ID => 0x107;

        public override string DisplayName => "Coal";

        public TimeSpan BurnTime => TimeSpan.FromSeconds(80);

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 0);
        }
    }
}