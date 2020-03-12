using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class RainforestBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Rainforest;

        public override double Temperature => 1.2f;

        public override double Rainfall => 0.9f;

        public override TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Birch, TreeSpecies.Oak}; }
        }

        public override double TreeDensity => 2;

        public override PlantSpecies[] Plants
        {
            get { return new[] {PlantSpecies.Fern, PlantSpecies.TallGrass}; }
        }
    }
}