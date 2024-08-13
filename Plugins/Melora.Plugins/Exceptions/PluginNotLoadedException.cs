using Melora.Plugins.Models;

namespace Melora.Plugins.Exceptions;

/// <summary>
/// Represents errors that occurr during loading plugins.
/// </summary>
/// <remarks>
/// Creates a new instance of PluginNotLoadedException
/// </remarks>
/// <param name="pluginPath">The path to the plugin archive attempted to load.</param>
/// <param name="pluginType">The type of the plugin attempted to load.</param>
/// <param name="manifest">The manifest of the plugin attempted to load if exists.</param>
/// <param name="innerException">The inner exception.</param>
public class PluginNotLoadedException(
    string pluginPath,
    Type? pluginType = null,
    Manifest? manifest = null,
    Exception? innerException = null) : Exception("Plugin could not be loaded.", innerException)
{
    /// <summary>
    /// The path to the plugin archive attempted to load.
    /// </summary>
    public string PluginPath { get; } = pluginPath;

    /// <summary>
    /// The type of the plugin attempted to load.
    /// </summary>
    public Type? PluginType { get; } = pluginType;

    /// <summary>
    /// The manifest of the plugin attempted to load if exists.
    /// </summary>
    public Manifest? Manifest { get; } = manifest;
}