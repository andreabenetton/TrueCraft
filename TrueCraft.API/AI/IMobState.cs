using TrueCraft.API.Entities;
using TrueCraft.API.Server;

namespace TrueCraft.API.AI
{
    public interface IMobState
    {
        void Update(IMobEntity entity, IEntityManager manager);
    }
}