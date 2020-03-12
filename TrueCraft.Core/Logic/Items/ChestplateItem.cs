using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ChestplateItem : ArmorItem, ICraftingRecipe
    {
        public override sbyte MaximumStack => 1;

        public ItemStack[,] Pattern
        {
            get
            {
                short baseMaterial = 0;
                switch (Material)
                {
                    case ArmorMaterial.Diamond:
                        baseMaterial = DiamondItem.ItemID;
                        break;
                    case ArmorMaterial.Gold:
                        baseMaterial = GoldIngotItem.ItemID;
                        break;
                    case ArmorMaterial.Iron:
                        baseMaterial = IronIngotItem.ItemID;
                        break;
                    case ArmorMaterial.Leather:
                        baseMaterial = LeatherItem.ItemID;
                        break;
                }

                return new[,]
                {
                    {new ItemStack(baseMaterial), ItemStack.EmptyStack, new ItemStack(baseMaterial)},
                    {new ItemStack(baseMaterial), new ItemStack(baseMaterial), new ItemStack(baseMaterial)},
                    {new ItemStack(baseMaterial), new ItemStack(baseMaterial), new ItemStack(baseMaterial)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class LeatherTunicItem : ChestplateItem
    {
        public static readonly short ItemID = 0x12B;

        public override short ID => 0x12B;

        public override ArmorMaterial Material => ArmorMaterial.Leather;

        public override short BaseDurability => 49;

        public override float BaseArmor => 4;

        public override string DisplayName => "Leather Tunic";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 1);
        }
    }

    public class IronChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x133;

        public override short ID => 0x133;

        public override ArmorMaterial Material => ArmorMaterial.Iron;

        public override short BaseDurability => 192;

        public override float BaseArmor => 4;

        public override string DisplayName => "Iron Chestplate";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 1);
        }
    }

    public class GoldenChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x13B;

        public override short ID => 0x13B;

        public override ArmorMaterial Material => ArmorMaterial.Gold;

        public override short BaseDurability => 96;

        public override float BaseArmor => 4;

        public override string DisplayName => "Golden Chestplate";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 1);
        }
    }

    public class DiamondChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x137;

        public override short ID => 0x137;

        public override ArmorMaterial Material => ArmorMaterial.Diamond;

        public override short BaseDurability => 384;

        public override float BaseArmor => 4;

        public override string DisplayName => "Diamond Chestplate";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 1);
        }
    }

    public class ChainChestplateItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x12F;

        public override short ID => 0x12F;

        public override ArmorMaterial Material => ArmorMaterial.Chain;

        public override short BaseDurability => 96;

        public override float BaseArmor => 4;

        public override string DisplayName => "Chain Chestplate";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 1);
        }
    }
}