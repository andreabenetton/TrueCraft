using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TrueCraft.Core;

namespace TrueCraft.Client
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            UserSettings.Local = new UserSettings();
            UserSettings.Local.Load();

            var user = new TrueCraftUser {Username = "andbene"};
            //var user = new TrueCraftUser { Username = args[1] };
            var client = new MultiplayerClient(user);
            var game = new TrueCraftGame(client, ParseEndPoint("127.0.0.1:25565")); //args[0]
            game.Run();
            client.Disconnect();
        }

        private static IPEndPoint ParseEndPoint(string arg)
        {
            IPAddress address;
            if (arg.Contains(':'))
            {
                // Both IP and port are specified
                var parts = arg.Split(':');
                if (!IPAddress.TryParse(parts[0], out address))
                    address = Resolve(parts[0]);
                return new IPEndPoint(address, int.Parse(parts[1]));
            }

            if (IPAddress.TryParse(arg, out address))
                return new IPEndPoint(address, 25565);
            if (int.TryParse(arg, out var port))
                return new IPEndPoint(IPAddress.Loopback, port);
            return new IPEndPoint(Resolve(arg), 25565);
        }

        private static IPAddress Resolve(string arg)
        {
            return Dns.GetHostEntry(arg).AddressList
                .FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}