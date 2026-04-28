using System;
using System.IO;
using System.Reflection;
using Xunit;
using TrueCraft.API;
using TrueCraft.Core.World;

namespace Test.TrueCraft.Core.World
{

    public class RegionTest
    {
        public Region Region { get; set; }

        public RegionTest()
        {
            var world = new global::TrueCraft.Core.World.World();
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Region = new Region(Coordinates2D.Zero, world,
                Path.Combine(assemblyDir, "Files", "r.0.0.mca"));
        }

        [Fact]
        public void TestGetChunk()
        {
            var chunk = Region.GetChunk(Coordinates2D.Zero);
            Assert.Equal(Coordinates2D.Zero, chunk.Coordinates);
            Assert.Throws<ArgumentException>(() =>
                Region.GetChunk(new Coordinates2D(31, 31)));
        }

        [Fact]
        public void TestUnloadChunk()
        {
            var chunk = Region.GetChunk(Coordinates2D.Zero);
            Assert.Equal(Coordinates2D.Zero, chunk.Coordinates);
            Assert.True(Region.Chunks.ContainsKey(Coordinates2D.Zero));
            Region.UnloadChunk(Coordinates2D.Zero);
            Assert.False(Region.Chunks.ContainsKey(Coordinates2D.Zero));
        }

        [Fact]
        public void TestGetRegionFileName()
        {
            Assert.Equal("r.0.0.mca", Region.GetRegionFileName(Region.Position));
        }
    }
}