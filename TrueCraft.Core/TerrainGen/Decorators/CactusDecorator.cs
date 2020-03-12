using System.Linq;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.TerrainGen.Decorations;
using TrueCraft.Core.TerrainGen.Noise;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class CactusDecorator : IChunkDecorator
    {
        public void Decorate(IWorld world, IChunk chunk, IBiomeRepository biomes)
        {
            var noise = new Perlin(world.Seed);
            var chanceNoise = new ClampNoise(noise);
            chanceNoise.MaxValue = 2;
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                var biome = biomes.GetBiome(chunk.Biomes[x * Chunk.Width + z]);
                var blockX = MathHelper.ChunkToBlockX(x, chunk.Coordinates.X);
                var blockZ = MathHelper.ChunkToBlockZ(z, chunk.Coordinates.Z);
                var height = chunk.HeightMap[x * Chunk.Width + z];
                if (biome.Plants.Contains(PlantSpecies.Cactus) && chanceNoise.Value2D(blockX, blockZ) > 1.7)
                {
                    var blockLocation = new Coordinates3D(x, height, z);
                    var cactiPosition = blockLocation + Coordinates3D.Up;
                    if (chunk.GetBlockID(blockLocation).Equals(SandBlock.BlockID))
                    {
                        var HeightChance = chanceNoise.Value2D(blockX, blockZ);
                        var CactusHeight = HeightChance < 1.4 ? 2 : 3;
                        Decoration.GenerateColumn(chunk, cactiPosition, CactusHeight, CactusBlock.BlockID);
                    }
                }
            }
        }
    }
}