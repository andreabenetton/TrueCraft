using System;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class LiquidDecorator : IChunkDecorator
    {
        public static readonly int WaterLevel = 40;

        public void Decorate(IWorld world, IChunk chunk, IBiomeRepository biomes)
        {
            for (var x = 0; x < Chunk.Width; x++)
            for (var z = 0; z < Chunk.Depth; z++)
            {
                var biome = biomes.GetBiome(chunk.Biomes[x * Chunk.Width + z]);
                var height = chunk.HeightMap[x * Chunk.Width + z];
                for (var y = height; y <= WaterLevel; y++)
                {
                    var blockLocation = new Coordinates3D(x, y, z);
                    int blockId = chunk.GetBlockID(blockLocation);
                    if (blockId.Equals(AirBlock.BlockID))
                    {
                        chunk.SetBlockID(blockLocation, biome.WaterBlock);
                        var below = blockLocation + Coordinates3D.Down;
                        if (!chunk.GetBlockID(below).Equals(AirBlock.BlockID) &&
                            !chunk.GetBlockID(below).Equals(biome.WaterBlock))
                            if (!biome.WaterBlock.Equals(LavaBlock.BlockID) &&
                                !biome.WaterBlock.Equals(StationaryLavaBlock.BlockID))
                            {
                                var random = new Random(world.Seed);
                                if (random.Next(100) < 40)
                                    chunk.SetBlockID(below, ClayBlock.BlockID);
                                else
                                    chunk.SetBlockID(below, SandBlock.BlockID);
                            }
                    }
                }

                for (var y = 4; y < height / 8; y++)
                {
                    var blockLocation = new Coordinates3D(x, y, z);
                    int blockId = chunk.GetBlockID(blockLocation);
                    if (blockId.Equals(AirBlock.BlockID)) chunk.SetBlockID(blockLocation, LavaBlock.BlockID);
                }
            }
        }
    }
}