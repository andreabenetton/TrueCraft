using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class RainforestBiome : BiomeProvider
    {
        public override byte ID
        {
            get { return (byte)Biome.Rainforest; }
        }

        public override double Temperature
        {
            get { return 1.2f; }
        }

        public override double Rainfall
        {
            get { return 0.9f; }
        }

        public override TreeSpecies[] Trees
        {
            get
            {
                return new[] { TreeSpecies.Birch, TreeSpecies.Oak };
            }
        }

        public override double TreeDensity
        {
            get
            {
                return 2;
            }
        }

        public override PlantSpecies[] Plants
        {
            get
            {
                return new[] { PlantSpecies.Fern, PlantSpecies.TallGrass };
            }
        }
    }
}