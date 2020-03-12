using TrueCraft.API;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.TerrainGen.Biomes
{
    public class DesertBiome : BiomeProvider
    {
        public override byte ID => (byte) Biome.Desert;

        public override double Temperature => 2.0f;

        public override double Rainfall => 0.0f;

        public override bool Spawn => false;

        public override TreeSpecies[] Trees => new TreeSpecies[0];

        public override PlantSpecies[] Plants
        {
            get { return new[] {PlantSpecies.Deadbush, PlantSpecies.Cactus}; }
        }

        public override byte SurfaceBlock => SandBlock.BlockID;

        public override byte FillerBlock => SandstoneBlock.BlockID;

        public override int SurfaceDepth => 4;
    }
}