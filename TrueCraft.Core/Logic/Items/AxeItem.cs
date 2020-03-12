using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class AxeItem : ToolItem, ICraftingRecipe
    {
        public override ToolType ToolType => ToolType.Axe;

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
                    {new ItemStack(baseMaterial), new ItemStack(baseMaterial)},
                    {new ItemStack(baseMaterial), new ItemStack(StickItem.ItemID)},
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class WoodenAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x10F;

        public override short ID => 0x10F;

        public override ToolMaterial Material => ToolMaterial.Wood;

        public override short BaseDurability => 60;

        public override string DisplayName => "Wooden Axe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 7);
        }
    }

    public class StoneAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x113;

        public override short ID => 0x113;

        public override ToolMaterial Material => ToolMaterial.Stone;

        public override short BaseDurability => 132;

        public override string DisplayName => "Stone Axe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 7);
        }
    }

    public class IronAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x102;

        public override short ID => 0x102;

        public override ToolMaterial Material => ToolMaterial.Iron;

        public override short BaseDurability => 251;

        public override string DisplayName => "Iron Axe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 7);
        }
    }

    public class GoldenAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x11E;

        public override short ID => 0x11E;

        public override ToolMaterial Material => ToolMaterial.Gold;

        public override short BaseDurability => 33;

        public override string DisplayName => "Golden Axe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 7);
        }
    }

    public class DiamondAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x117;

        public override short ID => 0x117;

        public override ToolMaterial Material => ToolMaterial.Diamond;

        public override short BaseDurability => 1562;

        public override string DisplayName => "Diamond Axe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 7);
        }
    }
}