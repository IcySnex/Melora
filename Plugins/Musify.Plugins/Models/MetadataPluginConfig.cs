using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Abstract;

namespace Musify.Plugins.Models;

/// <summary>
/// Describes a configuration for a metadata plugin
/// </summary>
public partial class MetadataPluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Creates a new MetadataPluginConfig
    /// </summary>
    /// <param name="items">Additional config items for the plugin.</param>
    public MetadataPluginConfig(
        PluginConfigItem[] items)
    {
        this.items = items;

        foreach (PluginConfigItem item in items)
            item.PropertyChanged += (s, e) =>
                OnPropertyChanged(item.Name);
    }


    /// <summary>
    /// Additional config items for the plugin.
    /// </summary>
    [ObservableProperty]
    PluginConfigItem[] items;
}