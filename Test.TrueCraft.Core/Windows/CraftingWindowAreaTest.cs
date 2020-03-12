using Moq;
using NUnit.Framework;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;

namespace Test.TrueCraft.Core.Windows
{
    [TestFixture]
    public class CraftingWindowAreaTest
    {
        [Test]
        public void TestCraftingWindowArea()
        {
            var recipe = new Mock<ICraftingRecipe>();
            recipe.Setup(r => r.Output).Returns(new ItemStack(10));
            var repository = new Mock<ICraftingRepository>();
            repository.Setup(r => r.GetRecipe(It.IsAny<IWindowArea>())).Returns(recipe.Object);

            var area = new CraftingWindowArea(repository.Object, 0) {[0] = new ItemStack(11)};
            Assert.AreEqual(new ItemStack(10), area[CraftingWindowArea.CraftingOutput]);
        }
    }
}