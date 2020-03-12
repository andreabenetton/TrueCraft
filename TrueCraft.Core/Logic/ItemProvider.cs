using System;
using TrueCraft.API;
using TrueCraft.API.Entities;
using TrueCraft.API.Logic;
using TrueCraft.API.Networking;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic
{
    public abstract class ItemProvider : IItemProvider
    {
        public abstract short ID { get; }

        public abstract Tuple<int, int> GetIconTexture(byte metadata);

        public virtual sbyte MaximumStack => 64;

        public virtual string DisplayName => string.Empty;

        public virtual void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnBlock(Coordinates3D coordinates, ItemStack item, BlockFace face, IWorld world,
            IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnNothing(ItemStack item, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }
    }
}