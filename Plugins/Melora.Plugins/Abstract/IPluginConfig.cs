using Melora.Plugins.Exceptions;
using Melora.Plugins.Models;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Describes a configuration for a plugin
/// </summary>
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
}