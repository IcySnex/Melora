using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;
using System.ComponentModel;

namespace Musify.Plugins.Models;

/// <summary>
/// Describes a configuration for a platfrom support plugin
/// </summary>
public partial class PlatformSupportPluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Should not be used to initialize a new PlatformSupportPluginConfig. This constructor is only used for serializing.
    /// </summary>
    [Obsolete("Should not be used to initialize a new PlatformSupportPluginConfig. This constructor is only used for serializing.")]
    public PlatformSupportPluginConfig()
    { }

    /// <summary>
    /// Creates a new PlatformSupportPluginConfig
    /// </summary>
    /// <param name="defaultItems">Default additional config items for the plugin.</param>
    /// <param name="defaultQuality">The default quality in which tracks get downloaded.</param>
    /// <param name="defaultFormat">The default format in which tracks get downloaded.</param>
    /// <param name="defaultSearchResultsLimit">The default limit of search results to fetch.</param>
    /// <param name="defaultSearchResultsSorting">The default sorting of search results.</param>
    /// <param name="defaultSearchResultsSortDescending">The default view options of the search page for the platform.</param>
    /// <param name="initialConfig">The config used for initializing if exists.</param>
    public PlatformSupportPluginConfig(
        PluginConfigItem[] defaultItems,
        Quality defaultQuality,
        Format defaultFormat,
        int? defaultSearchResultsLimit,
        Sorting defaultSearchResultsSorting,
        bool defaultSearchResultsSortDescending,
        PlatformSupportPluginConfig? initialConfig = null)
    {
        this.defaultItems = defaultItems;
        this.defaultQuality = defaultQuality;
        this.defaultFormat = defaultFormat;
        this.defaultSearchResultsLimit = defaultSearchResultsLimit;
        this.defaultSearchResultsSorting = defaultSearchResultsSorting;
        this.defaultSearchResultsSortDescending = defaultSearchResultsSortDescending;

        if (initialConfig is null)
        {
            Reset();
            return;
        }

        Items = initialConfig.Items.Select(item => (PluginConfigItem)item.Clone()).ToArray();
        Quality = initialConfig.Quality;
        Format = initialConfig.Format;
        SearchResultsLimit = initialConfig.SearchResultsLimit;
        SearchResultsSorting = initialConfig.SearchResultsSorting;
        SearchResultsSortDescending = initialConfig.SearchResultsSortDescending;
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
    /// The quality in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Quality quality = default!;
    readonly Quality defaultQuality = default!;

    /// <summary>
    /// The format in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Format format = default!;
    readonly Format defaultFormat = default!;

    /// <summary>
    /// The limit of search results to fetch.
    /// </summary>
    [ObservableProperty]
    int? searchResultsLimit = default!;
    readonly int? defaultSearchResultsLimit = default!;

    /// <summary>
    /// The sorting of search results.
    /// </summary>
    [ObservableProperty]
    Sorting searchResultsSorting = default!;
    readonly Sorting defaultSearchResultsSorting = default!;

    /// <summary>
    /// Weither search results are sorted descending or not.
    /// </summary>
    [ObservableProperty]
    bool searchResultsSortDescending = default!;
    readonly bool defaultSearchResultsSortDescending = default!;


    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    public void Reset()
    {
        Items = defaultItems.Select(item => (PluginConfigItem)item.Clone()).ToArray();
        Quality = defaultQuality;
        Format = defaultFormat;
        SearchResultsLimit = defaultSearchResultsLimit;
        SearchResultsSorting = defaultSearchResultsSorting;
        SearchResultsSortDescending = defaultSearchResultsSortDescending;
    }
}