using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SnowBlock : BlockProvider, ICraftingRecipe
    {
        public static readonly byte BlockID = 0x50;

        public override byte ID => 0x50;

        public override double BlastResistance => 1;

        public override double Hardness => 0.2;

        public override byte Luminance => 0;

        public override string DisplayName => "Snow Block";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Snow;

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(SnowballItem.ItemID), new ItemStack(SnowballItem.ItemID)},
                    {new ItemStack(SnowballItem.ItemID), new ItemStack(SnowballItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 4);
        }
    }

    public class SnowfallBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x4E;

        public override byte ID => 0x4E;

        public override double BlastResistance => 0.5;

        public override double Hardness => 0.6;

        public override byte Luminance => 0;

        public override bool RenderOpaque => true;

        public override bool Opaque => false;

        public override string DisplayName => "Snow";

        public override BoundingBox? BoundingBox => null;

        public override SoundEffectClass SoundEffect => SoundEffectClass.Snow;

        // TODO: This is metadata-aware
        public override BoundingBox? InteractiveBoundingBox =>
            new BoundingBox(Vector3.Zero, new Vector3(1, 1 / 16.0, 1));

        public override ToolType EffectiveTools => ToolType.Shovel;

        public override Coordinates3D GetSupportDirection(BlockDescriptor descriptor)
        {
            return Coordinates3D.Down;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 4);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] {new ItemStack(SnowballItem.ItemID)};
        }
    }
}