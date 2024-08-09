using Microsoft.Extensions.Logging;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;

namespace Musify.Plugins.Abstract;

/// <summary>
/// Represents a plugin which writes track metadata after downloading.
/// </summary>
/// <remarks>
/// Creates a new MetadataPlugin.
/// </remarks>
/// <param name="name">The name of the plugin.</param>
/// <param name="iconPathData">The path date for the plugin icon.</param>
/// <param name="config">The config the plugin gets initialized with.</param>
/// <param name="logger">An optional logger.</param>
public abstract class MetadataPlugin(
    string name,
    string iconPathData,
    MetadataPluginConfig config,
    ILogger<IPlugin>? logger = null) : IPlugin
{
    /// <summary>
    /// Generic logger used to log stuff lol.
    /// </summary>
    readonly protected ILogger<IPlugin>? logger = logger;


    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    public PluginKind Kind => PluginKind.Metadata;

    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The path date for the plugin icon.
    /// </summary>
    public string IconPathData { get; } = iconPathData;


    /// <summary>
    /// The config for the plugin.
    /// </summary>
    public MetadataPluginConfig Config { get; } = config;

    IPluginConfig IPlugin.Config => Config;


    /// <summary>
    /// Writes the metadate from the downloadable track to the file.
    /// </summary>
    /// <param name="filePath">The path to the audio file.</param>
    /// <param name="track">The downloadable track containing the metadata.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    public abstract Task WriteAsync(
        string filePath,
        DownloadableTrack track,
        CancellationToken cancellationToken = default);
}