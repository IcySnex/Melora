using Microsoft.Extensions.Logging;
using Musify.Plugins.Models;

namespace Musify.Plugins.Abstract;

/// <summary>
/// Represents a plugin which writes track metadata after downloading.
/// </summary>
public abstract class MetadatePlugin : IPlugin
{
    /// <summary>
    /// Creates a new MetadatePlugin.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="iconPathData">The path date for the plugin icon.</param>
    /// <param name="config">The config the plugin gets initialized with.</param>
    /// <param name="defaultConfig">The function to create a default config.</param>
    /// <param name="logger">An optional logger.</param>
    public MetadatePlugin(
        string name,
        string iconPathData,
        IPluginConfig? config,
        Func<MetadatePluginConfig> defaultConfig,
        ILogger<PlatformSupportPlugin>? logger = null)
    {
        this.Name = name;
        this.IconPathData = iconPathData;
        this.defaultConfig = defaultConfig;
        this.logger = logger;


        if (config is MetadatePluginConfig metadataConfig)
        {
            Config = metadataConfig;
            return;
        }
        MetadatePluginConfig defaultMetdataConfig = defaultConfig.Invoke();
        if (config is not null)
            defaultMetdataConfig.Items = config.Items;

        Config = defaultMetdataConfig;
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
    /// The path date for the plugin icon.
    /// </summary>
    public string IconPathData { get; }


    /// <summary>
    /// Gets the default config of the plugin.
    /// </summary>
    /// <returns>A new PlatformSupportPluginConfig</returns>
    public MetadatePluginConfig GetDefaultConfig() =>
        defaultConfig.Invoke();

    IPluginConfig IPlugin.GetDefaultConfig() =>
        GetDefaultConfig();


    readonly Func<MetadatePluginConfig> defaultConfig;

    /// <summary>
    /// The config for the plugin.
    /// </summary>
    public MetadatePluginConfig Config { get; }

    IPluginConfig IPlugin.Config => Config;
}