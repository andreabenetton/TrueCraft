using System;
using System.Diagnostics;
using System.Threading;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Sessions;

/// <summary>
///     One live (server, client) pair tracked by <see cref="GameSessionRegistry"/>.
///     <see cref="Server"/> is null for multiplayer sessions where the server lives
///     in another process. <see cref="Stop"/> is idempotent and fires
///     <see cref="Exited"/> exactly once, either when the user stops the session
///     explicitly or when the client process exits on its own.
/// </summary>
public sealed class GameSession
{
    private static int _nextId;
    private int _stopped;

    public GameSession(string label, SingleplayerServer server, Process client, string worldPath)
    {
        Id = Interlocked.Increment(ref _nextId);
        Label = label;
        Server = server;
        Client = client;
        WorldPath = worldPath;
        StartedAt = DateTime.Now;

        client.Exited += (_, _) => Stop();
    }

    public int Id { get; }
    public string Label { get; }
    public SingleplayerServer Server { get; }
    public Process Client { get; }

    /// <summary>Filesystem path of the world dir; null for multiplayer sessions.</summary>
    public string WorldPath { get; }

    public DateTime StartedAt { get; }

    /// <summary>Raised exactly once after Stop() completes its teardown.</summary>
    public event Action<GameSession> Exited;

    public void Stop()
    {
        if (Interlocked.Exchange(ref _stopped, 1) != 0)
            return;

        // Tear down in the same order the previous in-view OnClientExited
        // did: kill the client first so the server sees a clean disconnect,
        // then stop the server, then persist. Every step is best-effort —
        // a failure in one stage shouldn't prevent the others from running.
        try { if (!Client.HasExited) Client.Kill(entireProcessTree: true); } catch { }
        try { Server?.Stop(); } catch { }
        try { Server?.World.Save(); } catch { }

        Exited?.Invoke(this);
    }
}
