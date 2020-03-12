using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ShovelItem : ToolItem, ICraftingRecipe
    {
        public override ToolType ToolType => ToolType.Shovel;

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
                    {new ItemStack(baseMaterial)},
                    {new ItemStack(StickItem.ItemID)},
                    {new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class WoodenShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x10D;

        public override short ID => 0x10D;

        public override ToolMaterial Material => ToolMaterial.Wood;

        public override short BaseDurability => 60;

        public override string DisplayName => "Wooden Shovel";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 5);
        }
    }

    public class StoneShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x111;

        public override short ID => 0x111;

        public override ToolMaterial Material => ToolMaterial.Stone;

        public override short BaseDurability => 132;

        public override string DisplayName => "Stone Shovel";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 5);
        }
    }

    public class IronShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x100;

        public override short ID => 0x100;

        public override ToolMaterial Material => ToolMaterial.Iron;

        public override short BaseDurability => 251;

        public override string DisplayName => "Iron Shovel";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 5);
        }
    }

    public class GoldenShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x11C;

        public override short ID => 0x11C;

        public override ToolMaterial Material => ToolMaterial.Gold;

        public override short BaseDurability => 33;

        public override string DisplayName => "Golden Shovel";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 5);
        }
    }

    public class DiamondShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x115;

        public override short ID => 0x115;

        public override ToolMaterial Material => ToolMaterial.Diamond;

        public override short BaseDurability => 1562;

        public override string DisplayName => "Diamond Shovel";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 5);
        }
    }
}