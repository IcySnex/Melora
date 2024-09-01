using Melora.Plugins.Exceptions;
using Melora.Plugins.Models;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Describes a configuration for a plugin
/// </summary>
[JsonDerivedType(typeof(PlatformSupportPluginConfig), typeDiscriminator: "Melora.Plugins.Models.PlatformSupportPluginConfig")]
[JsonDerivedType(typeof(MetadataPluginConfig), typeDiscriminator: "Melora.Plugins.Models.MetadataPluginConfig")]
public interface IPluginConfig
{
    /// <summary>
    /// Additional config items for the plugin.
    /// </summary>
    PluginConfigItem[] Items { get; set; }

    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    void Reset();
}


/// <summary>
/// Contains extension methods for plugin configs.
/// </summary>
public static class PluginConfigExtensions
{
    /// <summary>
    /// Gets an item of a plugin config with the name.
    /// </summary>
    /// <typeparam name="T">The requsted type of the item.</typeparam>
    /// <param name="config">The config to get the item from.</param>
    /// <param name="name">The name of the requested item.</param>
    /// <returns>The requested items value.</returns>
    /// <exception cref="PluginConfigInvalidItemException">Occurrs when the item could not be found or does not represent the requested type.</exception>
    public static T GetItem<T>(
        this IPluginConfig config,
        string name)
    {
        PluginConfigItem item = config.Items.FirstOrDefault(item => item.Name == name) ?? throw new PluginConfigInvalidItemException(name, config, new("Name was not found."));

        if (item.Value is not T)
            throw new PluginConfigInvalidItemException(name, config, new("Item value does not represent requested type."));

        return (T)item.Value;
    }

    /// <summary>
    /// Checks if the items match the given array.
    /// </summary>
    /// <param name="config">The initial config.</param>
    /// <param name="items">The items to check for.</param>
    /// <returns>A bool representing the comparison.</returns>
    public static bool MatchesItemsOf(
        this IPluginConfig config,
        PluginConfigItem[] items) =>
        config.Items.Length == items.Length && config.Items.All(item => items.Any(defaultItem =>
        {
            if (item.Name != defaultItem.Name)
                return false;

            if (item.Value is null || defaultItem.Value is null)
                return true;

            return item.Value.GetType() == defaultItem.Value.GetType();
        }));
}