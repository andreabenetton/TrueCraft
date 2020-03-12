using TrueCraft.API;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class PlainsBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Plains;

        public override double Temperature => 0.8f;

        public override double Rainfall => 0.4f;

        public override TreeSpecies[] Trees
        {
            get { return new[] {TreeSpecies.Oak}; }
        }
    }
}