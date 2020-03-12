using System;
using TrueCraft.API.World;

namespace TrueCraft.Core.TerrainGen.Noise
{
    public class ModifyNoise : NoiseGen
    {
        public ModifyNoise(INoise primaryNoise, INoise secondaryNoise, NoiseModifier modifier = NoiseModifier.Add)
        {
            PrimaryNoise = primaryNoise;
            SecondaryNoise = secondaryNoise;
            Modifier = modifier;
        }

        public INoise PrimaryNoise { get; set; }
        public INoise SecondaryNoise { get; set; }
        public NoiseModifier Modifier { get; set; }

        public override double Value2D(double x, double y)
        {
            switch (Modifier)
            {
                case NoiseModifier.Add:
                    return PrimaryNoise.Value2D(x, y) + SecondaryNoise.Value2D(x, y);
                case NoiseModifier.Multiply:
                    return PrimaryNoise.Value2D(x, y) * SecondaryNoise.Value2D(x, y);
                case NoiseModifier.Power:
                    return Math.Pow(PrimaryNoise.Value2D(x, y), SecondaryNoise.Value2D(x, y));
                case NoiseModifier.Subtract:
                    return PrimaryNoise.Value2D(x, y) - SecondaryNoise.Value2D(x, y);
                default:
                    //This is unreachable.
                    return PrimaryNoise.Value2D(x, y) + SecondaryNoise.Value2D(x, y);
            }
        }

        public override double Value3D(double x, double y, double z)
        {
            switch (Modifier)
            {
                case NoiseModifier.Add:
                    return PrimaryNoise.Value3D(x, y, z) + SecondaryNoise.Value3D(x, y, z);
                case NoiseModifier.Multiply:
                    return PrimaryNoise.Value3D(x, y, z) * SecondaryNoise.Value3D(x, y, z);
                case NoiseModifier.Power:
                    return Math.Pow(PrimaryNoise.Value3D(x, y, z), SecondaryNoise.Value3D(x, y, z));
                case NoiseModifier.Subtract:
                    return PrimaryNoise.Value3D(x, y, z) - SecondaryNoise.Value3D(x, y, z);
                default:
                    //This is unreachable.
                    return PrimaryNoise.Value3D(x, y, z) + SecondaryNoise.Value3D(x, y, z);
            }
        }
    }

    public enum NoiseModifier
    {
        Add,
        Subtract,
        Multiply,
        Power
    }
}