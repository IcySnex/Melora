using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Abstract;

namespace Musify.Plugins.Models;

/// <summary>
/// Describes a configuration for a metadata plugin
/// </summary>
public partial class MetadatePluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Creates a new MetadatePluginConfig
    /// </summary>
    /// <param name="items">Additional config items for the plugin.</param>
    public MetadatePluginConfig(
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