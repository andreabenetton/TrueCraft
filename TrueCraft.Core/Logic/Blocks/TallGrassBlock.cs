using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class TallGrassBlock : BlockProvider
    {
        public enum TallGrassType
        {
            DeadBush = 0,
            TallGrass = 1,
            Fern = 2
        }

        public static readonly byte BlockID = 0x1F;

        public override byte ID => 0x1F;

        public override double BlastResistance => 0;

        public override double Hardness => 0;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override string DisplayName => "Tall Grass";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Grass;

        public override bool Flammable => true;

        public override BoundingBox? BoundingBox => null;

        public override BoundingBox? InteractiveBoundingBox => new BoundingBox(new Vector3(4 / 16.0), Vector3.One);

        public override Coordinates3D GetSupportDirection(BlockDescriptor descriptor)
        {
            return Coordinates3D.Down;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(7, 2);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            if (MathHelper.Random.Next(1, 24) == 1)
                return new[] {new ItemStack(SeedsItem.ItemID, 1)};
            return new[] {ItemStack.EmptyStack};
        }
    }
}