using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class BootsItem : ArmorItem, ICraftingRecipe
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
                    {new ItemStack(baseMaterial), ItemStack.EmptyStack, new ItemStack(baseMaterial)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class LeatherBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x12D;

        public override short ID => 0x12D;

        public override ArmorMaterial Material => ArmorMaterial.Leather;

        public override short BaseDurability => 40;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Leather Boots";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 3);
        }
    }

    public class IronBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x135;

        public override short ID => 0x135;

        public override ArmorMaterial Material => ArmorMaterial.Iron;

        public override short BaseDurability => 160;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Iron Boots";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 3);
        }
    }

    public class GoldenBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x13D;

        public override short ID => 0x13D;

        public override ArmorMaterial Material => ArmorMaterial.Gold;

        public override short BaseDurability => 80;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Golden Boots";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 3);
        }
    }

    public class DiamondBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x139;

        public override short ID => 0x139;

        public override ArmorMaterial Material => ArmorMaterial.Diamond;

        public override short BaseDurability => 320;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Diamond Boots";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 3);
        }
    }

    public class ChainBootsItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x131;

        public override short ID => 0x131;

        public override ArmorMaterial Material => ArmorMaterial.Chain;

        public override short BaseDurability => 79;

        public override float BaseArmor => 1.5f;

        public override string DisplayName => "Chain Boots";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 3);
        }
    }
}