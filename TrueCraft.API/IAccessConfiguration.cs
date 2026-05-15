namespace TrueCraft.API
{
    /// <summary>
    ///     Interface for objects providing server access configuration.
    /// </summary>
    public interface IAccessConfiguration
    {
        /// <summary>
        ///     Gets the list of blacklisted players.
        /// </summary>
        string[] Blacklist { get; }

        /// <summary>
        ///     Gets the list of whitelisted players.
        /// </summary>
        string[] Whitelist { get; }

        /// <summary>
        ///     Gets the list of opped players.
        /// </summary>
        string[] Oplist { get; }
    }
}
