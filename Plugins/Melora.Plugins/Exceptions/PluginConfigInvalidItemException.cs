using Melora.Plugins.Abstract;

namespace Melora.Plugins.Exceptions;

/// <summary>
/// Represents errors that occurr when an requested item of a plugin config is invalid in.
/// </summary>
/// <remarks>
/// Creates a new instance of PluginConfigInvalidItemException
/// </remarks>
/// <param name="requestedItem">The name of the rquested config item.</param>
/// <param name="config">The config from which the item was requested.</param>
/// <param name="innerException">The inner exception.</param>
public class PluginConfigInvalidItemException(
    string requestedItem,
    IPluginConfig config,
    Exception? innerException = null) : Exception("Item of plugin config is invalid.", innerException)
{
    /// <summary>
    /// The name of the rquested config item.
    /// </summary>
    public string RequestedItem { get; } = requestedItem;

    /// <summary>
    /// The config from which the item was requested.
    /// </summary>
    public IPluginConfig Config { get; } = config;
}