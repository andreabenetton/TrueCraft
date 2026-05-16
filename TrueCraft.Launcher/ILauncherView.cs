using System;
using GeonBit.UI.Entities;

namespace TrueCraft.Launcher;

/// <summary>
///     A pane the launcher shell mounts into its interaction panel. Each view owns
///     its own GeonBit.UI entity tree and is fully removed from the UI by clearing
///     its host panel's children.
/// </summary>
public interface ILauncherView : IDisposable
{
    void Mount(Panel parent);
}
