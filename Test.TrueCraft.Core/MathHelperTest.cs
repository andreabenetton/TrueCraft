using Xunit;
using TrueCraft.API;
using TrueCraft.Core;

namespace Test.TrueCraft.Core;


public class MathHelperTest
{
    [Fact]
    public void TestCreateRotationByte()
    {
        byte a = (byte)MathHelper.CreateRotationByte(0);
        byte b = (byte)MathHelper.CreateRotationByte(180);
        byte c = (byte)MathHelper.CreateRotationByte(359);
        byte d = (byte)MathHelper.CreateRotationByte(360);
        Assert.Equal(0, a);
        Assert.Equal(128, b);
        Assert.Equal(255, c);
        Assert.Equal(0, d);
    }

    [Fact]
    public void TestGetCollisionPoint()
    {
        var inputs = new[]
        {
            Vector3.Down,
            Vector3.Up,
            Vector3.Left,
            Vector3.Right,
            Vector3.Forwards,
            Vector3.Backwards
        };
        var results = new[]
        {
            MathHelper.GetCollisionPoint(inputs[0]),
            MathHelper.GetCollisionPoint(inputs[1]),
            MathHelper.GetCollisionPoint(inputs[2]),
            MathHelper.GetCollisionPoint(inputs[3]),
            MathHelper.GetCollisionPoint(inputs[4]),
            MathHelper.GetCollisionPoint(inputs[5])
        };
        var expected = new[]
        {
            CollisionPoint.NegativeY,
            CollisionPoint.PositiveY,
            CollisionPoint.NegativeX,
            CollisionPoint.PositiveX,
            CollisionPoint.PositiveZ,
            CollisionPoint.NegativeZ
        };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], results[i]);
        }
    }
}
