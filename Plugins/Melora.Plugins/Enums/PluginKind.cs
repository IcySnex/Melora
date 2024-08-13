namespace Melora.Plugins.Enums;

/// <summary>
/// Describes what kind a plugin is.
/// </summary>
public enum PluginKind
{
    /// <summary>
    /// Platform support plugin: Adds support for additional platforms.
    /// </summary>
    PlatformSupport = 0,

    /// <summary>
    /// Metadata plugin: Writes track metadata after downloading.
    /// </summary>
    Metadata = 1,
}