﻿using System;
using System.Linq;
using TrueCraft.API;
using TrueCraft.API.Entities;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.Physics;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Nbt.Tags;

namespace TrueCraft.Core.Logic
{
    /// <summary>
    ///     Provides common implementations of block logic.
    /// </summary>
    public abstract class BlockProvider : IBlockProvider
    {
        public static readonly byte[] Overwritable =
        {
            AirBlock.BlockID,
            WaterBlock.BlockID,
            StationaryWaterBlock.BlockID,
            LavaBlock.BlockID,
            StationaryLavaBlock.BlockID,
            SnowfallBlock.BlockID
        };

        public static IBlockRepository BlockRepository { get; set; }
        public static IItemRepository ItemRepository { get; set; }

        public virtual void BlockLeftClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            var coords = descriptor.Coordinates + MathHelper.BlockFaceToCoordinates(face);
            if (world.IsValidPosition(coords) && world.GetBlockID(coords) == FireBlock.BlockID)
                world.SetBlockID(coords, 0);
        }

        public virtual bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            return true;
        }

        public virtual void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void BlockMined(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            GenerateDropEntity(descriptor, world, user.Server, user.SelectedItem);
            world.SetBlockID(descriptor.Coordinates, 0);
        }

        public void GenerateDropEntity(BlockDescriptor descriptor, IWorld world, IMultiplayerServer server,
            ItemStack item)
        {
            var entityManager = server.GetEntityManagerForWorld(world);
            var items = new ItemStack[0];
            var type = ToolType.None;
            var material = ToolMaterial.None;
            var held = ItemRepository.GetItemProvider(item.ID);

            if (held is ToolItem tool)
            {
                material = tool.Material;
                type = tool.ToolType;
            }

            if ((EffectiveTools & type) > 0)
                if ((EffectiveToolMaterials & material) > 0)
                    items = GetDrop(descriptor, item);

            foreach (var i in items)
            {
                if (i.Empty) continue;
                var entity = new ItemEntity(new Vector3(descriptor.Coordinates) + new Vector3(0.5), i);
                entityManager.SpawnEntity(entity);
            }
        }

        public virtual void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server,
            IWorld world)
        {
            if (!IsSupported(descriptor, server, world))
            {
                GenerateDropEntity(descriptor, world, server, ItemStack.EmptyStack);
                world.SetBlockID(descriptor.Coordinates, 0);
            }
        }

        public virtual void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnNothing(ItemStack item, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            var old = world.GetBlockData(coordinates);
            if (Overwritable.All(b => b != old.ID))
            {
                coordinates += MathHelper.BlockFaceToCoordinates(face);
                old = world.GetBlockData(coordinates);
                if (Overwritable.All(b => b != old.ID))
                    return;
            }

            // Test for entities
            if (BoundingBox.HasValue)
            {
                var em = user.Server.GetEntityManagerForWorld(world);
                var entities = em.EntitiesInRange(coordinates, 3);
                var box = new BoundingBox(BoundingBox.Value.Min + (Vector3) coordinates,
                    BoundingBox.Value.Max + (Vector3) coordinates);
                foreach (var entity in entities)
                {
                    if (entity is IAABBEntity aabb && !(entity is ItemEntity))
                        if (aabb.BoundingBox.Intersects(box))
                            return;

                    if (entity is PlayerEntity player)
                        if (new BoundingBox(player.Position, player.Position + player.Size)
                            .Intersects(box))
                            return;
                }
            }

            // Place block
            world.SetBlockID(coordinates, ID);
            world.SetMetadata(coordinates, (byte) item.Metadata);

            BlockPlaced(world.GetBlockData(coordinates), face, world, user);

            if (!IsSupported(world.GetBlockData(coordinates), user.Server, world))
            {
                world.SetBlockData(coordinates, old);
            }
            else
            {
                item.Count--;
                user.Inventory[user.SelectedSlot] = item;
            }
        }

        public virtual void BlockLoadedFromChunk(Coordinates3D coords, IMultiplayerServer server, IWorld world)
        {
            // This space intentionally left blank
        }

        public virtual void TileEntityLoadedForClient(BlockDescriptor descriptor, IWorld world, NbtCompound entity,
            IRemoteClient client)
        {
            // This space intentionally left blank
        }

        short IItemProvider.ID => ID;

        /// <summary>
        ///     The ID of the block.
        /// </summary>
        public abstract byte ID { get; }

        public virtual Tuple<int, int> GetIconTexture(byte metadata)
        {
            return null; // Blocks are rendered in 3D
        }

        public virtual SoundEffectClass SoundEffect => SoundEffectClass.Stone;

        /// <summary>
        ///     The maximum amount that can be in a single stack of this block.
        /// </summary>
        public virtual sbyte MaximumStack => 64;

        /// <summary>
        ///     How resist the block is to explosions.
        /// </summary>
        public virtual double BlastResistance => 0;

        /// <summary>
        ///     How resist the block is to player mining/digging.
        /// </summary>
        public virtual double Hardness => 0;

        /// <summary>
        ///     The light level emitted by the block. 0 - 15
        /// </summary>
        public virtual byte Luminance => 0;

        /// <summary>
        ///     Whether or not the block is opaque
        /// </summary>
        public virtual bool Opaque => true;

        /// <summary>
        ///     Whether or not the block is rendered opaque
        /// </summary>
        public virtual bool RenderOpaque => Opaque;

        public virtual bool Flammable => false;

        /// <summary>
        ///     The amount removed from the light level as it passes through this block.
        ///     255 - Let no light pass through(this may change)
        ///     Notes:
        ///     - This isn't needed for opaque blocks
        ///     - This is needed since some "partial" transparent blocks remove more than 1 level from light passing through such
        ///     as Ice.
        /// </summary>
        public virtual byte LightOpacity
        {
            get
            {
                if (Opaque)
                    return 255;
                return 0;
            }
        }

        public virtual bool DiffuseSkyLight => false;

        /// <summary>
        ///     The name of the block as it would appear to players.
        /// </summary>
        public virtual string DisplayName => string.Empty;

        public virtual ToolMaterial EffectiveToolMaterials => ToolMaterial.All;

        public virtual ToolType EffectiveTools => ToolType.All;

        public virtual Tuple<int, int> GetTextureMap(byte metadata)
        {
            return null;
        }

        public virtual BoundingBox? BoundingBox => new BoundingBox(Vector3.Zero, Vector3.One);

        public virtual BoundingBox? InteractiveBoundingBox => BoundingBox;

        public virtual bool IsSupported(BlockDescriptor descriptor, IMultiplayerServer server, IWorld world)
        {
            var support = GetSupportDirection(descriptor);
            if (support != Coordinates3D.Zero)
            {
                var supportingBlock =
                    server.BlockRepository.GetBlockProvider(world.GetBlockID(descriptor.Coordinates + support));
                if (!supportingBlock.Opaque)
                    return false;
            }

            return true;
        }

        protected virtual ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            short meta;
            if (this is ICraftingRecipe)
                meta = (short) (((ICraftingRecipe) this).SignificantMetadata ? descriptor.Metadata : 0);
            else
                meta = descriptor.Metadata;
            return new[] {new ItemStack(descriptor.ID, 1, meta)};
        }

        public virtual Coordinates3D GetSupportDirection(BlockDescriptor descriptor)
        {
            return Coordinates3D.Zero;
        }

        /// <summary>
        ///     Gets the time required to mine the given block with the given item.
        /// </summary>
        /// <returns>The harvest time in milliseconds.</returns>
        /// <param name="blockId">Block identifier.</param>
        /// <param name="itemId">Item identifier.</param>
        /// <param name="damage">Damage sustained by the item.</param>
        public static int GetHarvestTime(byte blockId, short itemId, out short damage)
        {
            // Reference:
            // http://minecraft.gamepedia.com/index.php?title=Breaking&oldid=138286

            damage = 0;

            var block = BlockRepository.GetBlockProvider(blockId);
            var item = ItemRepository.GetItemProvider(itemId);

            var hardness = block.Hardness;
            if (hardness == -1)
                return -1;

            var time = hardness * 1.5;

            if (item is ToolItem unknown)
            {
                var tool = unknown.ToolType;
                var material = unknown.Material;

                if ((block.EffectiveTools & tool) == 0 || (block.EffectiveToolMaterials & material) == 0)
                    time *= 3.33; // Add time for ineffective tools
                if (material != ToolMaterial.None)
                    switch (material)
                    {
                        case ToolMaterial.Wood:
                            time /= 2;
                            break;
                        case ToolMaterial.Stone:
                            time /= 4;
                            break;
                        case ToolMaterial.Iron:
                            time /= 6;
                            break;
                        case ToolMaterial.Diamond:
                            time /= 8;
                            break;
                        case ToolMaterial.Gold:
                            time /= 12;
                            break;
                    }

                damage = 1;
                if (tool == ToolType.Shovel || tool == ToolType.Axe || tool == ToolType.Pickaxe)
                {
                    damage = (short) (hardness != 0 ? 1 : 0);
                }
                else if (tool == ToolType.Sword)
                {
                    damage = (short) (hardness != 0 ? 2 : 0);
                    time /= 1.5;
                    if (block is CobwebBlock)
                        time /= 1.5;
                }
                else if (tool == ToolType.Hoe)
                {
                    damage = 0; // What? This doesn't seem right
                }
                else if (unknown is ShearsItem)
                {
                    if (block is WoolBlock)
                        time /= 5;
                    else if (block is LeavesBlock || block is CobwebBlock)
                        time /= 15;
                    if (block is LeavesBlock || block is CobwebBlock || block is TallGrassBlock)
                        damage = 1;
                    else
                        damage = 0;
                }
            }

            return (int) (time * 1000);
        }
    }
}