using System.Collections.Generic;
using TrueCraft.API.Networking;

namespace TrueCraft.API.Server
{
    public interface ICommandManager
    {
        IList<ICommand> Commands { get; }
        void HandleCommand(IRemoteClient Client, string Alias, string[] Arguments);
    }
}