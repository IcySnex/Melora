using Melora.Plugins.Enums;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Represents a plugin for additional platform support.
/// </summary>
/// <remarks>
/// Creates a new PlatformSupportPlugin.
/// </remarks>
/// <param name="name">The name of the plugin.</param>
/// <param name="iconPathData">The path date for the plugin icon.</param>
/// <param name="config">The config the plugin gets initialized with.</param>
/// <param name="logger">An optional logger.</param>
public abstract class PlatformSupportPlugin(
    string name,
    string iconPathData,
    PlatformSupportPluginConfig config,
    ILogger<IPlugin>? logger = null) : IPlugin
{
    /// <summary>
    /// Generic logger used to log stuff lol.
    /// </summary>
    readonly protected ILogger<IPlugin>? logger = logger;


    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    public PluginKind Kind => PluginKind.PlatformSupport;

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
    public PlatformSupportPluginConfig Config { get; } = config;

    IPluginConfig IPlugin.Config => Config;


    /// <summary>
    /// Searches for a query on the platform.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="progress">The progress to report to.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>An enumerable containing the search results.</returns>
    public abstract Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares search results for downloads.
    /// </summary>
    /// <param name="searchResults">The search results to prepare for downloads.</param>
    /// <param name="progress">The progress to report to.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>An enumerable containing the downloadble tracks</returns>
    public abstract Task<IEnumerable<DownloadableTrack>> PrepareDownloadsAsync(
        IEnumerable<SearchResult> searchResults,
        IProgress<string> progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the stream of a downloadable track.
    /// </summary>
    /// <param name="track">The downloadable track to get the stream from.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>The downloadable tracks stream.</returns>
    public abstract Task<Stream> GetStreamAsync(
        DownloadableTrack track,
        CancellationToken cancellationToken = default);
}