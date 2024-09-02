namespace Melora.Plugins.Exceptions;

/// <summary>
/// Represents errors that occurr when an requested option of a plugin config is invalid.
/// </summary>
/// <remarks>
/// Creates a new instance of PluginOptionException
/// </remarks>
/// <param name="requestedOption">The name of the rquested config option.</param>
/// <param name="innerException">The inner exception.</param>
public class PluginOptionException(
    string requestedOption,
    Exception? innerException = null) : Exception("Option of plugin config is invalid.", innerException)
{
    /// <summary>
    /// The name of the rquested config option.
    /// </summary>
    public string RequestedOption { get; } = requestedOption;
}