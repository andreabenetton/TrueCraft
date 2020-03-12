using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Windows;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CraftingTableBlock : BlockProvider, ICraftingRecipe, IBurnableItem
    {
        public static readonly byte BlockID = 0x3A;

        public override byte ID => 0x3A;

        public override double BlastResistance => 12.5;

        public override double Hardness => 2.5;

        public override byte Luminance => 0;

        public override string DisplayName => "Crafting Table";

        public override SoundEffectClass SoundEffect => SoundEffectClass.Wood;

        public TimeSpan BurnTime => TimeSpan.FromSeconds(15);

        public ItemStack[,] Pattern
        {
            get
            {
                return new[,]
                {
                    {new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID)},
                    {new ItemStack(WoodenPlanksBlock.BlockID), new ItemStack(WoodenPlanksBlock.BlockID)}
                };
            }
        }

        public ItemStack Output => new ItemStack(BlockID);

        public bool SignificantMetadata => false;

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            var window = new CraftingBenchWindow(user.Server.CraftingRepository, (InventoryWindow) user.Inventory);
            user.OpenWindow(window);
            window.Disposed += (sender, e) =>
            {
                var entityManager = user.Server.GetEntityManagerForWorld(world);
                for (var i = 0; i < window.CraftingGrid.StartIndex + window.CraftingGrid.Length; i++)
                {
                    var item = window[i];
                    if (!item.Empty)
                    {
                        var entity = new ItemEntity(descriptor.Coordinates + Coordinates3D.Up, item);
                        entityManager.SpawnEntity(entity);
                    }
                }
            };
            return false;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(11, 3);
        }
    }
}