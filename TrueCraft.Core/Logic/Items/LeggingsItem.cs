using System;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class LeggingsItem : ArmorItem, ICraftingRecipe
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
                    {new ItemStack(baseMaterial), ItemStack.EmptyStack, new ItemStack(baseMaterial)},
                    {new ItemStack(baseMaterial), ItemStack.EmptyStack, new ItemStack(baseMaterial)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class LeatherPantsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x12C;

        public override short ID => 0x12C;

        public override ArmorMaterial Material => ArmorMaterial.Leather;

        public override short BaseDurability => 46;

        public override float BaseArmor => 3;

        public override string DisplayName => "Leather Pants";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 2);
        }
    }

    public class IronLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x134;

        public override short ID => 0x134;

        public override ArmorMaterial Material => ArmorMaterial.Iron;

        public override short BaseDurability => 184;

        public override float BaseArmor => 3;

        public override string DisplayName => "Iron Leggings";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 2);
        }
    }

    public class GoldenLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x13C;

        public override short ID => 0x13C;

        public override ArmorMaterial Material => ArmorMaterial.Gold;

        public override short BaseDurability => 92;

        public override float BaseArmor => 3;

        public override string DisplayName => "Golden Leggings";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 2);
        }
    }

    public class DiamondLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x138;

        public override short ID => 0x138;

        public override ArmorMaterial Material => ArmorMaterial.Diamond;

        public override short BaseDurability => 368;

        public override float BaseArmor => 3;

        public override string DisplayName => "Diamond Leggings";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 2);
        }
    }

    public class ChainLeggingsItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x130;

        public override short ID => 0x130;

        public override ArmorMaterial Material => ArmorMaterial.Chain;

        public override short BaseDurability => 92;

        public override float BaseArmor => 3;

        public override string DisplayName => "Chain Leggings";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 2);
        }
    }
}