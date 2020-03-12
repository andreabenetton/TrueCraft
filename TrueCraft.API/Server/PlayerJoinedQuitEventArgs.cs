using System;
using TrueCraft.API.Networking;

namespace TrueCraft.API.Server
{
    public class PlayerJoinedQuitEventArgs : EventArgs
    {
        public PlayerJoinedQuitEventArgs(IRemoteClient client)
        {
            Client = client;
        }

        public IRemoteClient Client { get; set; }
    }
}