using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class HelmentItem : ArmorItem, ICraftingRecipe
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
                    {new ItemStack(baseMaterial), new ItemStack(baseMaterial), new ItemStack(baseMaterial)},
                    {new ItemStack(baseMaterial), ItemStack.EmptyStack, new ItemStack(baseMaterial)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class LeatherCapItem : HelmentItem
    {
        public static readonly short ItemID = 0x12A;

        public override short ID => 0x12A;

        public override ArmorMaterial Material => ArmorMaterial.Leather;

        public override short BaseDurability => 34;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Leather Cap";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 0);
        }
    }

    public class IronHelmetItem : HelmentItem
    {
        public static readonly short ItemID = 0x132;

        public override short ID => 0x132;

        public override ArmorMaterial Material => ArmorMaterial.Iron;

        public override short BaseDurability => 136;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Iron Helmet";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 0);
        }
    }

    public class GoldenHelmetItem : HelmentItem
    {
        public static readonly short ItemID = 0x13A;

        public override short ID => 0x13A;

        public override ArmorMaterial Material => ArmorMaterial.Gold;

        public override short BaseDurability => 68;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Golden Helmet";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 0);
        }
    }

    public class DiamondHelmetItem : HelmentItem
    {
        public static readonly short ItemID = 0x136;

        public override short ID => 0x136;

        public override ArmorMaterial Material => ArmorMaterial.Diamond;

        public override short BaseDurability => 272;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Diamond Helmet";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 0);
        }
    }

    public class ChainHelmetItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x12E;

        public override short ID => 0x12E;

        public override ArmorMaterial Material => ArmorMaterial.Chain;

        public override short BaseDurability => 67;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Chain Helmet";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 0);
        }
    }
}