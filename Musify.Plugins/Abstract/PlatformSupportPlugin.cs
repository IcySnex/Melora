using Microsoft.Extensions.Logging;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;

namespace Musify.Plugins.Abstract;

/// <summary>
/// Represents a plugin for additional platform support.
/// </summary>
public abstract class PlatformSupportPlugin : IPlugin
{
    /// <summary>
    /// Creates a new PlatformSupportPlugin.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="iconPathData">The path date for the plugin icon.</param>
    /// <param name="config">The config the plugin gets initialized with.</param>
    /// <param name="defaultConfig">The function to create a default config.</param>
    /// <param name="logger">An optional logger.</param>
    public PlatformSupportPlugin(
        string name,
        string iconPathData,
        IPluginConfig? config,
        Func<PlatformSupportPluginConfig> defaultConfig,
        ILogger<PlatformSupportPlugin>? logger = null)
    {
        this.Name = name;
        this.IconPathData = iconPathData;
        this.defaultConfig = defaultConfig;
        this.logger = logger;


        if (config is PlatformSupportPluginConfig platformSupportConfig)
        {
            Config = platformSupportConfig;
            return;
        }
        PlatformSupportPluginConfig defaultPlatformSupportConfig = defaultConfig.Invoke();
        if (config is not null)
            defaultPlatformSupportConfig.Items = config.Items;

        Config = defaultPlatformSupportConfig;
    }


    /// <summary>
    /// Generic logger used to log stuff lol.
    /// </summary>
    readonly protected ILogger<PlatformSupportPlugin>? logger;


    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    public PluginKind Kind { get; } = PluginKind.PlatformSupport;

    /// <summary>
    /// The path date for the plugin icon.
    /// </summary>
    public string IconPathData { get; }


    /// <summary>
    /// Gets the default config of the plugin.
    /// </summary>
    /// <returns>A new PlatformSupportPluginConfig</returns>
    public PlatformSupportPluginConfig GetDefaultConfig() =>
        defaultConfig.Invoke();

    IPluginConfig IPlugin.GetDefaultConfig() =>
        GetDefaultConfig();


    readonly Func<PlatformSupportPluginConfig> defaultConfig;

    /// <summary>
    /// The config for the plugin.
    /// </summary>
    public PlatformSupportPluginConfig Config { get; }

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