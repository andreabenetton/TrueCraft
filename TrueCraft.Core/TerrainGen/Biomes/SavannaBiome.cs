using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class SavannaBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Savanna;

        public override double Temperature => 1.2f;

        public override double Rainfall => 0.0f;

        public override TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Oak}; }
        }

        public override PlantSpecies[] Plants
        {
            get { return new[] {PlantSpecies.Deadbush, PlantSpecies.Fern}; }
        }

        public override double TreeDensity => 50;
    }
}