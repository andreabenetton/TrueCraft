using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class TaigaBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Taiga;

        public override double Temperature => 0.0f;

        public override double Rainfall => 0.0f;

        public override TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Spruce}; }
        }

        public override double TreeDensity => 5;
    }
}