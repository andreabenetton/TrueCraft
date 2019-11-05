using System.Collections.Generic;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen
{
    public class EmptyGenerator : IChunkProvider
    {
        public IChunk GenerateChunk(IWorld world, Coordinates2D coordinates)
        {
            return new Chunk(coordinates);
        }

        public Coordinates3D GetSpawn(IWorld world)
        {
            return Coordinates3D.Zero;
        }

        public void Initialize(IWorld world)
        {
        }

        public IList<IChunkDecorator> ChunkDecorators { get; set; }
    }
}