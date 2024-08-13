using Melora.Plugins.Enums;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Represents a plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    PluginKind Kind { get; }

    /// <summary>
    /// The name the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The path date for the plugin icon.
    /// </summary>
    string IconPathData { get; }


    /// <summary>
    /// The config for the plugin.
    /// </summary>
    IPluginConfig Config { get; }
}