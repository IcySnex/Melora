using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;

namespace Musify.Plugins.Models;

/// <summary>
/// Describes a configuration for a platfrom support plugin
/// </summary>
/// <remarks>
/// Creates a new PlatformSupportPluginConfig
/// </remarks>
public partial class PlatformSupportPluginConfig : ObservableObject, IPluginConfig
{
    /// <param name="items">Additional config items for the plugin.</param>
    /// <param name="quality">The quality in which tracks get downloaded.</param>
    /// <param name="format">The format in which tracks get downloaded.</param>
    /// <param name="searchResultsLimit">The limit of search results to fetch.</param>
    /// <param name="searchResultsSorting">The sorting of search results.</param>
    /// <param name="searchResultsSortDescending">The view options of the search page for the platform.</param>
    public PlatformSupportPluginConfig(
        PluginConfigItem[] items,
        Quality quality,
        Format format,
        int? searchResultsLimit,
        Sorting searchResultsSorting,
        bool searchResultsSortDescending)
    {
        this.items = items;
        this.quality = quality;
        this.format = format;
        this.searchResultsLimit = searchResultsLimit;
        this.searchResultsSorting = searchResultsSorting;
        this.searchResultsSortDescending = searchResultsSortDescending;

        foreach (PluginConfigItem item in items)
            item.PropertyChanged += (s, e) =>
                OnPropertyChanged(item.Name);
    }


    /// <summary>
    /// Additional config items for the plugin.
    /// </summary>
    [ObservableProperty]
    PluginConfigItem[] items;

    /// <summary>
    /// The quality in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Quality quality;

    /// <summary>
    /// The format in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Format format;

    /// <summary>
    /// The limit of search results to fetch.
    /// </summary>
    [ObservableProperty]
    int? searchResultsLimit;

    /// <summary>
    /// The sorting of search results.
    /// </summary>
    [ObservableProperty]
    Sorting searchResultsSorting;

    /// <summary>
    /// Weither search results are sorted descending or not.
    /// </summary>
    [ObservableProperty]
    bool searchResultsSortDescending;
}