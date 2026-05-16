namespace Iguina.Defs;

/// <summary>
/// Selector for one of Iguina's bundled theme variants.
/// Used by <see cref="UISystem.LoadBuiltinTheme"/> to map an enum value to
/// the corresponding <c>system_style.json</c> path under a theme-root
/// directory.
///
/// Iguina ships theme assets on the filesystem rather than embedded in
/// the DLL — much smaller binary, and the texture files are mutable so
/// callers can drop in their own customizations. <see cref="UISystem.LoadBuiltinTheme"/>
/// takes a <c>themesRoot</c> path under which subdirectories matching the
/// enum names are expected.
/// </summary>
public enum BuiltinThemes
{
    /// <summary>The "LowRes" pixel-art theme — see <c>Iguina.LowResTheme/</c>.</summary>
    LowRes,
}
