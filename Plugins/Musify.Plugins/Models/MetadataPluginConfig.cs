using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;
using System.ComponentModel;

namespace Musify.Plugins.Models;

/// <summary>
/// Describes a configuration for a metadata plugin
/// </summary>
public partial class MetadataPluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Should not be used to initialize a new MetadataPluginConfig. This constructor is only used for serializing.
    /// </summary>
    [Obsolete("Should not be used to initialize a new MetadataPluginConfig. This constructor is only used for serializing.")]
    public MetadataPluginConfig()
    { }

    /// <summary>
    /// Creates a new MetadataPluginConfig
    /// </summary>
    /// <param name="defaultItems">Default additional config items for the plugin.</param>
    /// <param name="initialConfig">The config used for initializing if exists.</param>
    public MetadataPluginConfig(
        PluginConfigItem[] defaultItems,
        MetadataPluginConfig? initialConfig = null)
    {
        this.defaultItems = defaultItems;

        if (initialConfig is null)
        {
            Reset();
            return;
        }

        if (initialConfig.Items.Length != defaultItems.Length || !initialConfig.Items.All(item => defaultItems.Any(defaultItem => item.Name == defaultItem.Name && item.Value.GetType() == defaultItem.Value.GetType())))
            throw new PluginConfigInvalidException(this, new("Passed initial config does not match additional items requiered for the plugin."));

        Items = initialConfig.Items.Select(item => (PluginConfigItem)item.Clone()).ToArray();
    }


    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(((PluginConfigItem)sender!).Name);

    partial void OnItemsChanged(
        PluginConfigItem[]? oldValue,
        PluginConfigItem[] newValue)
    {
        if (oldValue is not null)
            foreach (PluginConfigItem item in oldValue)
                item.PropertyChanged -= OnItemPropertyChanged;

        foreach (PluginConfigItem item in newValue)
            item.PropertyChanged += OnItemPropertyChanged;
    }

    /// <summary>
    /// Additional config items for the plugin.
    /// </summary>
    [ObservableProperty]
    PluginConfigItem[] items = default!;
    readonly PluginConfigItem[] defaultItems = default!;


    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    public void Reset()
    {
        Items = defaultItems.Select(item => (PluginConfigItem)item.Clone()).ToArray();
    }
}