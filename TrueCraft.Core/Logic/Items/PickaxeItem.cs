using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class PickaxeItem : ToolItem, ICraftingRecipe
    {
        public override ToolType ToolType => ToolType.Pickaxe;

        public ItemStack[,] Pattern
        {
            get
            {
                short baseMaterial = 0;
                switch (Material)
                {
                    case ToolMaterial.Diamond:
                        baseMaterial = DiamondItem.ItemID;
                        break;
                    case ToolMaterial.Gold:
                        baseMaterial = GoldIngotItem.ItemID;
                        break;
                    case ToolMaterial.Iron:
                        baseMaterial = IronIngotItem.ItemID;
                        break;
                    case ToolMaterial.Stone:
                        baseMaterial = CobblestoneBlock.BlockID;
                        break;
                    case ToolMaterial.Wood:
                        baseMaterial = WoodenPlanksBlock.BlockID;
                        break;
                }

                return new[,]
                {
                    {new ItemStack(baseMaterial), new ItemStack(baseMaterial), new ItemStack(baseMaterial)},
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID), ItemStack.EmptyStack},
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID), ItemStack.EmptyStack}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class WoodenPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x10E;

        public override short ID => 0x10E;

        public override ToolMaterial Material => ToolMaterial.Wood;

        public override short BaseDurability => 60;

        public override string DisplayName => "Wooden Pickaxe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 6);
        }
    }

    public class StonePickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x112;

        public override short ID => 0x112;

        public override ToolMaterial Material => ToolMaterial.Stone;

        public override short BaseDurability => 132;

        public override string DisplayName => "Stone Pickaxe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 6);
        }
    }

    public class IronPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x101;

        public override short ID => 0x101;

        public override ToolMaterial Material => ToolMaterial.Iron;

        public override short BaseDurability => 251;

        public override string DisplayName => "Iron Pickaxe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 6);
        }
    }

    public class GoldenPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x11D;

        public override short ID => 0x11D;

        public override ToolMaterial Material => ToolMaterial.Gold;

        public override short BaseDurability => 33;

        public override string DisplayName => "Golden Pickaxe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 6);
        }
    }

    public class DiamondPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x116;

        public override short ID => 0x116;

        public override ToolMaterial Material => ToolMaterial.Diamond;

        public override short BaseDurability => 1562;

        public override string DisplayName => "Diamond Pickaxe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 6);
        }
    }
}