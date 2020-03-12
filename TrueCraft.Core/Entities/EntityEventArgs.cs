using System;
using TrueCraft.API.Entities;

namespace TrueCraft.Core.Entities
{
    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; set; }
    }
}