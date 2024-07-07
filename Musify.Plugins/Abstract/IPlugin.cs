using Musify.Plugins.Enums;

namespace Musify.Plugins.Abstract;

/// <summary>
/// Represents a plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// The name the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    PluginKind Kind { get; }


    /// <summary>
    /// Test
    /// </summary>
    /// <param name="parameter">Test parameter.</param>
    Task TestAsync(
        string parameter);
}