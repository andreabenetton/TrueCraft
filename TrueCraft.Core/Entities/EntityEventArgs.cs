using System;
using TrueCraft.API.Entities;

namespace TrueCraft.Core.Entities
{
    public class EntityEventArgs : EventArgs
    {
        public IEntity Entity { get; set; }

        public EntityEventArgs(IEntity entity)
        {
            Entity = entity;
        }
    }
}
