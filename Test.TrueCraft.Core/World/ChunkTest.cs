using Xunit;
using TrueCraft.API;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace Test.TrueCraft.Core.World
{

    public class ChunkTest
    {
        [Fact]
        public void TestGetBlockID()
        {
            var chunk = new Chunk();
            chunk.SetBlockID(Coordinates3D.Zero, 12);
            Assert.Equal(12, chunk.GetBlockID(Coordinates3D.Zero));
        }

        [Fact]
        public void TestGetBlockLight()
        {
            var chunk = new Chunk();
            chunk.SetBlockLight(Coordinates3D.Zero, 5);
            Assert.Equal(5, chunk.GetBlockLight(Coordinates3D.Zero));
        }

        [Fact]
        public void TestGetSkyLight()
        {
            var chunk = new Chunk();
            chunk.SetSkyLight(Coordinates3D.Zero, 5);
            Assert.Equal(5, chunk.GetSkyLight(Coordinates3D.Zero));
        }

        [Fact]
        public void TestGetMetadata()
        {
            var chunk = new Chunk();
            chunk.SetMetadata(Coordinates3D.Zero, 5);
            Assert.Equal(5, chunk.GetMetadata(Coordinates3D.Zero));
        }

        [Fact]
        public void TestHeightMap()
        {
            var chunk = new Chunk();
            for (int x = 0; x < Chunk.Width; ++x)
            for (int z = 0; z < Chunk.Width; ++z)
                chunk.SetBlockID(new Coordinates3D(x, 20, z), StoneBlock.BlockID);
            chunk.UpdateHeightMap();
            Assert.Equal(20, chunk.GetHeight(0, 0));
            Assert.Equal(20, chunk.GetHeight(1, 0));
            chunk.SetBlockID(new Coordinates3D(1, 80, 0), 1);
            Assert.Equal(80, chunk.GetHeight(1, 0));
        }

        [Fact]
        public void TestConsistency()
        {
            var chunk = new Chunk();
            byte val = 0;
            for (int y = 0; y < Chunk.Height; y++)
            for (int x = 0; x < Chunk.Width; x++)
            for (int z = 0; z < Chunk.Depth; z++)
            {
                var coords = new Coordinates3D(x, y, z);
                chunk.SetBlockID(coords, val);
                chunk.SetMetadata(coords, (byte)(val % 16));
                chunk.SetBlockLight(coords, (byte)(val % 16));
                chunk.SetSkyLight(coords, (byte)(val % 16));
                val++;
            }
            val = 0;
            for (int y = 0; y < Chunk.Height; y++)
            for (int x = 0; x < Chunk.Width; x++)
            for (int z = 0; z < Chunk.Depth; z++)
            {
                var coords = new Coordinates3D(x, y, z);
                Assert.Equal(val, chunk.GetBlockID(coords));
                Assert.Equal((byte)(val % 16), chunk.GetMetadata(coords));
                Assert.Equal((byte)(val % 16), chunk.GetBlockLight(coords));
                Assert.Equal((byte)(val % 16), chunk.GetSkyLight(coords));
                val++;
            }
        }
    }
}