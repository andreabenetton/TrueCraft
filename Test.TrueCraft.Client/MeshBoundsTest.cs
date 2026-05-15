using Microsoft.Xna.Framework;
using TrueCraft.Client.Rendering;
using Xunit;

namespace Test.TrueCraft.Client
{
    public class MeshBoundsTest
    {
        [Fact]
        public void ComputesAxisAlignedBoundsAroundOrigin()
        {
            var vertices = new[]
            {
                Vertex(-1, -2, -3),
                Vertex(4, 5, 6),
                Vertex(0, 0, 0),
            };

            var bounds = Mesh.ComputeAxisAlignedBounds(vertices);

            Assert.Equal(new Vector3(-1, -2, -3), bounds.Min);
            Assert.Equal(new Vector3(4, 5, 6), bounds.Max);
        }

        [Fact]
        public void ComputesAxisAlignedBoundsForOffOriginCluster()
        {
            // Regression: the previous implementation picked vertices by
            // Vector3.Length() (distance to origin), which produced a wrong
            // AABB any time the geometry did not straddle (0,0,0). Every
            // chunk except the one at world origin hit this case.
            var vertices = new[]
            {
                Vertex(100, 50, 100),
                Vertex(116, 50, 100),
                Vertex(116, 50, 116),
                Vertex(100, 50, 116),
                Vertex(100, 178, 100),
                Vertex(116, 178, 116),
            };

            var bounds = Mesh.ComputeAxisAlignedBounds(vertices);

            Assert.Equal(new Vector3(100, 50, 100), bounds.Min);
            Assert.Equal(new Vector3(116, 178, 116), bounds.Max);
        }

        [Fact]
        public void ComputesAxisAlignedBoundsForFullyNegativeCluster()
        {
            var vertices = new[]
            {
                Vertex(-200, -100, -200),
                Vertex(-184, -100, -200),
                Vertex(-184, -100, -184),
                Vertex(-200, 28, -184),
            };

            var bounds = Mesh.ComputeAxisAlignedBounds(vertices);

            Assert.Equal(new Vector3(-200, -100, -200), bounds.Min);
            Assert.Equal(new Vector3(-184, 28, -184), bounds.Max);
        }

        private static VertexPositionNormalColorTexture Vertex(float x, float y, float z)
        {
            return new VertexPositionNormalColorTexture(
                new Vector3(x, y, z), Vector3.Up, Color.White, Vector2.Zero);
        }
    }
}
