using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class ForestBiome : BiomeProvider
    {
        public override byte ID
        {
            get { return (byte)Biome.Forest; }
        }

        public override double Temperature
        {
            get { return 0.7f; }
        }

        public override double Rainfall
        {
            get { return 0.8f; }
        }

        public override PlantSpecies[] Plants
        {
            get
            {
                return new[] { PlantSpecies.TallGrass };
            }
        }
    }
}