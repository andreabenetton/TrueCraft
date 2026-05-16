namespace TrueCraft.Options;

/// <summary>
///     Strongly-typed bindings for the <c>profiler</c> section of <c>nodesettings.json</c>.
/// </summary>
public sealed class ProfilerOptions
{
    public const string SectionName = "profiler";

    public string Buckets { get; set; } = string.Empty;
    public bool Lag { get; set; } = false;
}
