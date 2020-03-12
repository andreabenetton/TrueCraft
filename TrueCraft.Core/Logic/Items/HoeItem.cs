using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class HoeItem : ToolItem, ICraftingRecipe
    {
        public override ToolType ToolType => ToolType.Hoe;

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
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID)},
                    {ItemStack.EmptyStack, new ItemStack(StickItem.ItemID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(ID);

        public bool SignificantMetadata => false;

        public override void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            var id = world.GetBlockID(coordinates);
            if (id == DirtBlock.BlockID || id == GrassBlock.BlockID)
            {
                world.SetBlockID(coordinates, FarmlandBlock.BlockID);
                user.Server.BlockRepository.GetBlockProvider(FarmlandBlock.BlockID).BlockPlaced(
                    new BlockDescriptor {Coordinates = coordinates}, face, world, user);
            }
        }
    }

    public class WoodenHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x122;

        public override short ID => 0x122;

        public override ToolMaterial Material => ToolMaterial.Wood;

        public override short BaseDurability => 60;

        public override string DisplayName => "Wooden Hoe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 8);
        }
    }

    public class StoneHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x123;

        public override short ID => 0x123;

        public override ToolMaterial Material => ToolMaterial.Stone;

        public override short BaseDurability => 132;

        public override string DisplayName => "Stone Hoe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 8);
        }
    }

    public class IronHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x124;

        public override short ID => 0x124;

        public override ToolMaterial Material => ToolMaterial.Iron;

        public override short BaseDurability => 251;

        public override string DisplayName => "Iron Hoe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 8);
        }
    }

    public class GoldenHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x126;

        public override short ID => 0x126;

        public override ToolMaterial Material => ToolMaterial.Gold;

        public override short BaseDurability => 33;

        public override string DisplayName => "Golden Hoe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 8);
        }
    }

    public class DiamondHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x125;

        public override short ID => 0x125;

        public override ToolMaterial Material => ToolMaterial.Diamond;

        public override short BaseDurability => 1562;

        public override string DisplayName => "Diamond Hoe";

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 8);
        }
    }
}