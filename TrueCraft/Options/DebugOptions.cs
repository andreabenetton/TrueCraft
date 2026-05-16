namespace TrueCraft.Options;

/// <summary>
///     Strongly-typed bindings for the <c>debug</c> section of <c>nodesettings.json</c>.
/// </summary>
public sealed class DebugOptions
{
    public const string SectionName = "debug";

    public bool DeleteWorldOnStartup { get; set; } = false;
    public bool DeletePlayersOnStartup { get; set; } = false;
}
