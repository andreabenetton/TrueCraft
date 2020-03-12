using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CobwebBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x1E;

        public override byte ID => 0x1E;

        public override double BlastResistance => 20;

        public override double Hardness => 4;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override bool DiffuseSkyLight => true;

        public override ToolType EffectiveTools => ToolType.Sword;

        public override string DisplayName => "Cobweb";

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(11, 0);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(StringItem.ItemID, 1, descriptor.Metadata)};
        }
    }
}