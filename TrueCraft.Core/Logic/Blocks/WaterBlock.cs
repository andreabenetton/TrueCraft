using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class WaterBlock : FluidBlock
    {
        public static readonly byte BlockID = 0x08;

        public override byte ID => 0x08;

        public override double BlastResistance => 500;

        public override double Hardness => 100;

        public override byte Luminance => 0;

        public override bool Opaque => false;

        public override byte LightOpacity => 2;

        public override string DisplayName => "Water";

        protected override double SecondsBetweenUpdates => 0.25;

        protected override byte MaximumFluidDepletion => 7;

        protected override byte FlowingID => BlockID;

        protected override byte StillID => StationaryWaterBlock.BlockID;

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(13, 12);
        }
    }

    public class StationaryWaterBlock : WaterBlock
    {
        public new static readonly byte BlockID = 0x09;

        public override byte ID => 0x09;

        public override string DisplayName => "Water (stationary)";
    }
}