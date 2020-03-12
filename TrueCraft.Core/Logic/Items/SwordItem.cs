using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class SwordItem : ToolItem, ICraftingRecipe
    {
        public abstract float Damage { get; }

        public override ToolType ToolType => ToolType.Sword;

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
                    {new ItemStack(baseMaterial)},
                    {new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;
    }

    public class WoodenSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x10C;

        public override short ID => 0x10C;

        public override ToolMaterial Material => ToolMaterial.Wood;

        public override short BaseDurability => 60;

        public override float Damage => 2.5f;

        public override string DisplayName => "Wooden Sword";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 4);
        }
    }

    public class StoneSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x110;

        public override short ID => 0x110;

        public override ToolMaterial Material => ToolMaterial.Stone;

        public override short BaseDurability => 132;

        public override float Damage => 3.5f;

        public override string DisplayName => "Stone Sword";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 4);
        }
    }

    public class IronSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x10B;

        public override short ID => 0x10B;

        public override ToolMaterial Material => ToolMaterial.Iron;

        public override short BaseDurability => 251;

        public override float Damage => 4.5f;

        public override string DisplayName => "Iron Sword";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 4);
        }
    }

    public class GoldenSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x11B;

        public override short ID => 0x11B;

        public override ToolMaterial Material => ToolMaterial.Gold;

        public override short BaseDurability => 33;

        public override float Damage => 2.5f;

        public override string DisplayName => "Golden Sword";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 4);
        }
    }

    public class DiamondSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x114;

        public override short ID => 0x114;

        public override ToolMaterial Material => ToolMaterial.Diamond;

        public override short BaseDurability => 1562;

        public override float Damage => 5.5f;

        public override string DisplayName => "Diamond Sword";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 4);
        }
    }
}