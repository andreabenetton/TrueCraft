using System.Linq;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.TerrainGen.Noise;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class PlantDecorator : IChunkDecorator
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
                if (noise.Value2D(blockX, blockZ) > 0)
                {
                    var blockLocation = new Coordinates3D(x, height, z);
                    var plantPosition = blockLocation + Coordinates3D.Up;
                    if (chunk.GetBlockID(blockLocation) == biome.SurfaceBlock && plantPosition.Y < Chunk.Height)
                    {
                        var chance = chanceNoise.Value2D(blockX, blockZ);
                        if (chance < 1)
                        {
                            var bushNoise = chanceNoise.Value2D(blockX * 0.7, blockZ * 0.7);
                            var grassNoise = chanceNoise.Value2D(blockX * 0.3, blockZ * 0.3);
                            if (biome.Plants.Contains(PlantSpecies.Deadbush) && bushNoise > 1 &&
                                chunk.GetBlockID(blockLocation) == SandBlock.BlockID)
                            {
                                GenerateDeadBush(chunk, plantPosition);
                                continue;
                            }

                            if (biome.Plants.Contains(PlantSpecies.TallGrass) && grassNoise > 0.3 && grassNoise < 0.95)
                            {
                                var meta = grassNoise > 0.3 && grassNoise < 0.45 &&
                                           biome.Plants.Contains(PlantSpecies.Fern)
                                    ? (byte) 0x2
                                    : (byte) 0x1;
                                GenerateTallGrass(chunk, plantPosition, meta);
                            }
                        }
                        else
                        {
                            var flowerTypeNoise = chanceNoise.Value2D(blockX * 1.2, blockZ * 1.2);
                            if (biome.Plants.Contains(PlantSpecies.Rose) && flowerTypeNoise > 0.8 &&
                                flowerTypeNoise < 1.5)
                                GenerateRose(chunk, plantPosition);
                            else if (biome.Plants.Contains(PlantSpecies.Dandelion) && flowerTypeNoise <= 0.8)
                                GenerateDandelion(chunk, plantPosition);
                        }
                    }
                }
            }
        }

        private void GenerateRose(IChunk chunk, Coordinates3D location)
        {
            chunk.SetBlockID(location, RoseBlock.BlockID);
        }

        private void GenerateDandelion(IChunk chunk, Coordinates3D location)
        {
            chunk.SetBlockID(location, DandelionBlock.BlockID);
        }

        private void GenerateTallGrass(IChunk chunk, Coordinates3D location, byte meta)
        {
            chunk.SetBlockID(location, TallGrassBlock.BlockID);
            chunk.SetMetadata(location, meta);
        }

        private void GenerateDeadBush(IChunk chunk, Coordinates3D location)
        {
            chunk.SetBlockID(location, DeadBushBlock.BlockID);
        }
    }
}