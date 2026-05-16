using Xunit;
using TrueCraft.API;
using TrueCraft.Core.Windows;

namespace Test.TrueCraft.Core.Windows;


public class WindowAreaTest
{
    [Fact]
    public void TestIndexing()
    {
        var area = new WindowArea(0, 10, 10, 1);
        area[0] = new ItemStack(10);
        Assert.Equal(new ItemStack(10), area[0]);
        area[1] = new ItemStack(20);
        Assert.Equal(new ItemStack(20), area[1]);
        bool called = false;
        area.WindowChange += (sender, e) => called = true;
        area[0] = ItemStack.EmptyStack;
        Assert.True(called);
    }

    [Fact]
    public void TestCopyTo()
    {
        var area1 = new WindowArea(0, 10, 10, 1);
        var area2 = new WindowArea(0, 10, 10, 1);
        area1[0] = new ItemStack(10);
        area1[1] = new ItemStack(20);
        area1[2] = new ItemStack(30);
        area1.CopyTo(area2);
        Assert.Equal(area1[0], area2[0]);
        Assert.Equal(area1[1], area2[1]);
        Assert.Equal(area1[2], area2[2]);
    }
}

