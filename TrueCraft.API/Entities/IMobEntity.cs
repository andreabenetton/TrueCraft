using System;
using TrueCraft.API.AI;
using TrueCraft.API.Physics;

namespace TrueCraft.API.Entities
{
    public interface IMobEntity : IEntity, IAABBEntity
    {
        PathResult CurrentPath { get; set; }
        IMobState CurrentState { get; set; }
        event EventHandler PathComplete;
        bool AdvancePath(TimeSpan time, bool faceRoute = true);
        void Face(Vector3 target);
    }
}