using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using TrueCraft.Client.Handlers;
using TrueCraft.Core;

namespace TrueCraft.Client;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Minimal DI container — client has no settings file yet; logging
        // falls back to the default null sink.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<PacketHandlers>();
        App.Services = services.BuildServiceProvider();

        UserSettings.Local = UserSettings.Load();

        // Argument order: <server[:port]> <username>
        var endpoint = args.Length > 0
            ? args[0]
            : !string.IsNullOrWhiteSpace(UserSettings.Local.LastIP)
                ? UserSettings.Local.LastIP
                : "127.0.0.1:25565";

        var username = args.Length > 1
            ? args[1]
            : !string.IsNullOrWhiteSpace(UserSettings.Local.Username)
                ? UserSettings.Local.Username
                : "Player";

        var user = new TrueCraftUser { Username = username };
        var client = new MultiplayerClient(user,
            App.Services.GetRequiredService<PacketHandlers>());
        var game = new TrueCraftGame(client, ParseEndPoint(endpoint));
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
