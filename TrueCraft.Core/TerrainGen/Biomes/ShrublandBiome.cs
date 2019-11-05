using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class ShrublandBiome : BiomeProvider
    {
        public override byte ID
        {
            get { return (byte)Biome.Shrubland; }
        }

        public override double Temperature
        {
            get { return 0.8f; }
        }

        public override double Rainfall
        {
            get { return 0.4f; }
        }

        public override TreeSpecies[] Trees
        {
            get
            {
                return new[] { TreeSpecies.Oak };
            }
        }

        public override PlantSpecies[] Plants
        {
            get
            {
                return new PlantSpecies[0];
            }
        }
    }
}