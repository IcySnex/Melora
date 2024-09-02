using Melora.Plugins.Abstract;

namespace Melora.Plugins.Exceptions;

/// <summary>
/// Represents errors that occurr when a plugin config is invalid.
/// </summary>
/// <remarks>
/// Creates a new instance of PluginConfigException
/// </remarks>
/// <param name="config">The invalid config.</param>
/// <param name="innerException">The inner exception.</param>
public class PluginConfigException(
    IPluginConfig config,
    Exception? innerException = null) : Exception("Plugin config is invalid.", innerException)
{
    /// <summary>
    /// The invalid config.
    /// </summary>
    public IPluginConfig Config { get; } = config;
}