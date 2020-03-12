using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LapisLazuliOreBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x15;

        public override byte ID => 0x15;

        public override double BlastResistance => 15;

        public override double Hardness => 3;

        public override byte Luminance => 0;

        public override string DisplayName => "Lapis Lazuli Ore";

        public override ToolMaterial EffectiveToolMaterials =>
            ToolMaterial.Stone | ToolMaterial.Iron | ToolMaterial.Diamond;

        public override ToolType EffectiveTools => ToolType.Pickaxe;

        //public ItemStack SmeltingOutput { get { return new ItemStack(); } } // TODO: Metadata

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 10);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[]
                {new ItemStack(DyeItem.ItemID, (sbyte) new Random().Next(4, 8), (short) DyeItem.DyeType.LapisLazuli)};
        }
    }
}