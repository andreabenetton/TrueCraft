using System.IO;
using System.Reflection;
using Xunit;
using TrueCraft.API;
using TrueCraft.Core.TerrainGen;

namespace Test.TrueCraft.Core.World
{

    public class WorldTest
    {
        public global::TrueCraft.Core.World.World World { get; set; }

        public WorldTest()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            World = global::TrueCraft.Core.World.World.LoadWorld(Path.Combine(assemblyDir, "Files"));
        }

        [Fact]
        public void TestMetadataLoaded()
        {
            // Constants from manifest.nbt
            Assert.Equal(new Coordinates3D(0, 60, 0), World.SpawnPoint);
            Assert.Equal(1168393583, World.Seed);
            Assert.IsAssignableFrom<StandardGenerator>(World.ChunkProvider);
            Assert.Equal("default", World.Name);
        }

        [Fact]
        public void TestFindChunk()
        {
            var a = World.FindChunk(new Coordinates3D(0, 0, 0));
            var b = World.FindChunk(new Coordinates3D(-1, 0, 0));
            var c = World.FindChunk(new Coordinates3D(-1, 0, -1));
            var d = World.FindChunk(new Coordinates3D(16, 0, 0));
            Assert.Equal(new Coordinates2D(0, 0), a.Coordinates);
            Assert.Equal(new Coordinates2D(-1, 0), b.Coordinates);
            Assert.Equal(new Coordinates2D(-1, -1), c.Coordinates);
            Assert.Equal(new Coordinates2D(1, 0), d.Coordinates);
        }

        [Fact]
        public void TestGetChunk()
        {
            var a = World.GetChunk(new Coordinates2D(0, 0));
            var b = World.GetChunk(new Coordinates2D(1, 0));
            var c = World.GetChunk(new Coordinates2D(-1, 0));
            Assert.Equal(new Coordinates2D(0, 0), a.Coordinates);
            Assert.Equal(new Coordinates2D(1, 0), b.Coordinates);
            Assert.Equal(new Coordinates2D(-1, 0), c.Coordinates);
        }
    }
}