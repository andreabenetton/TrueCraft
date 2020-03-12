namespace TrueCraft.Core.Logic.Blocks
{
    public class LavaBlock : FluidBlock
    {
        public static readonly byte BlockID = 0x0A;

        public LavaBlock() : this(false)
        {
        }

        public LavaBlock(bool nether)
        {
            if (nether)
                _MaximumFluidDepletion = 7;
            else
                _MaximumFluidDepletion = 3;
        }

        public override byte ID => 0x0A;

        public override double BlastResistance => 0;

        public override double Hardness => 100;

        public override byte Luminance => 15;

        public override bool Opaque => false;

        public override byte LightOpacity => 255;

        public override string DisplayName => "Lava";

        protected override bool AllowSourceCreation => false;

        protected override double SecondsBetweenUpdates => 2;

        private byte _MaximumFluidDepletion { get; }
        protected override byte MaximumFluidDepletion => _MaximumFluidDepletion;

        protected override byte FlowingID => BlockID;

        protected override byte StillID => StationaryLavaBlock.BlockID;
    }

    public class StationaryLavaBlock : LavaBlock
    {
        public new static readonly byte BlockID = 0x0B;

        public override byte ID => 0x0B;

        public override double BlastResistance => 500;

        public override string DisplayName => "Lava (stationary)";
    }
}