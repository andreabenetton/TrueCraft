using TrueCraft.API;
using Xunit;

namespace Test.TrueCraft.API
{
    public class TestBoundingCylinder
    {
        [Fact]
        public void TestIntersectsPoint()
        {
            //   x
            //  /
            // x
            var cylinder = new BoundingCylinder(Vector3.Zero, Vector3.One, 1);
            Assert.True(cylinder.Intersects(cylinder.Min));
            Assert.True(cylinder.Intersects(cylinder.Max));
            Assert.True(cylinder.Intersects(cylinder.Min + (Vector3.One / 2)));
            Assert.True(cylinder.Intersects(cylinder.Max - (Vector3.One / 2)));
            Assert.True(cylinder.Intersects(new Vector3(0.25, 0, 0)));
            Assert.False(cylinder.Intersects(new Vector3(5, 5, 5)));
        }

        [Fact]
        public void TestIntersectsBox()
        {
            //   x
            //  /
            // x
            var cylinder = new BoundingCylinder(Vector3.Zero, Vector3.One * 10, 3);
            var doesNotIntersect = new BoundingBox(Vector3.One * 10 + 5, Vector3.One * 10 + 5);
            Assert.False(cylinder.Intersects(doesNotIntersect));
            var intersects = new BoundingBox(Vector3.Zero, Vector3.One);
            Assert.True(cylinder.Intersects(intersects));
        }
    }
}
