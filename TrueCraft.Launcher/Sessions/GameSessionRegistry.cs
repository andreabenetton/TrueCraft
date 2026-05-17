using System;
using System.Collections.Generic;

namespace TrueCraft.Launcher.Sessions;

/// <summary>
///     Thread-safe registry of <see cref="GameSession"/> instances owned by
///     <see cref="LauncherGame"/>. Subscribes to each session's
///     <see cref="GameSession.Exited"/> event and auto-removes; UI callers
///     subscribe to <see cref="Added"/> / <see cref="Removed"/> to redraw.
///     All event invocations are marshalled onto the launcher's UI thread
///     via <see cref="LauncherGame.Invoke"/> so handlers don't have to.
/// </summary>
public sealed class GameSessionRegistry
{
    private readonly LauncherGame _game;
    private readonly List<GameSession> _sessions = new();
    private readonly object _lock = new();

    public GameSessionRegistry(LauncherGame game)
    {
        _game = game;
    }

    public int Count
    {
        get { lock (_lock) return _sessions.Count; }
    }

    /// <summary>Snapshot of current sessions. Safe to enumerate; backed by a copy.</summary>
    public IReadOnlyList<GameSession> All
    {
        get { lock (_lock) return _sessions.ToArray(); }
    }

    public event Action<GameSession> Added;
    public event Action<GameSession> Removed;

    /// <summary>
    ///     Add a session. Refuses duplicates by <see cref="GameSession.WorldPath"/>
    ///     (so the same world cannot be launched twice). Returns false with a
    ///     user-facing error if rejected.
    /// </summary>
    public bool TryAdd(GameSession session, out string error)
    {
        lock (_lock)
        {
            if (!string.IsNullOrEmpty(session.WorldPath))
            {
                foreach (var existing in _sessions)
                    if (string.Equals(existing.WorldPath, session.WorldPath, StringComparison.Ordinal))
                    {
                        error = "World is already running.";
                        return false;
                    }
            }
            _sessions.Add(session);
        }

        // The session may fire Exited on a thread-pool thread (Process.Exited)
        // or on the UI thread (user clicked Stop). Marshal both paths through
        // Invoke so the registry's own list mutation + Removed handlers stay
        // on the UI thread.
        session.Exited += s => _game.Invoke(() => RemoveInternal(s));

        Added?.Invoke(session);
        error = null;
        return true;
    }

    public bool TryFindByWorldPath(string path, out GameSession session)
    {
        lock (_lock)
        {
            foreach (var s in _sessions)
                if (string.Equals(s.WorldPath, path, StringComparison.Ordinal))
                {
                    session = s;
                    return true;
                }
            session = null;
            return false;
        }
    }

    private void RemoveInternal(GameSession session)
    {
        bool removed;
        lock (_lock) removed = _sessions.Remove(session);
        if (removed)
            Removed?.Invoke(session);
    }
}
