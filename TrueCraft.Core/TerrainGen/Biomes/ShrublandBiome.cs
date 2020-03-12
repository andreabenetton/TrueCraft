using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class ShrublandBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Shrubland;

        public override double Temperature => 0.8f;

        public override double Rainfall => 0.4f;

        public override TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Oak}; }
        }

        public override PlantSpecies[] Plants => new PlantSpecies[0];
    }
}