using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Plugins.Exceptions;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Describes a configuration for a platfrom support plugin
/// </summary>
public partial class PlatformSupportPluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Should not be used to initialize a new PlatformSupportPluginConfig. This constructor is only used for serializing.
    /// </summary>
    [Obsolete("Should not be used to initialize a new PlatformSupportPluginConfig. This constructor is only used for serializing.")]
    [JsonConstructor]
    public PlatformSupportPluginConfig(
        IOption[] options,
        Quality quality,
        Format format,
        int? searchResultsLimit,
        Sorting searchResultsSorting,
        bool searchResultsSortDescending) : this(options, quality, format, searchResultsLimit, searchResultsSorting, searchResultsSortDescending, null)
    { }

    /// <summary>
    /// Creates a new PlatformSupportPluginConfig
    /// </summary>
    /// <param name="defaultOptions">Default additional config options for the plugin.</param>
    /// <param name="defaultQuality">The default quality in which tracks get downloaded.</param>
    /// <param name="defaultFormat">The default format in which tracks get downloaded.</param>
    /// <param name="defaultSearchResultsLimit">The default limit of search results to fetch.</param>
    /// <param name="defaultSearchResultsSorting">The default sorting of search results.</param>
    /// <param name="defaultSearchResultsSortDescending">The default view options of the search page for the platform.</param>
    /// <param name="initialConfig">The config used for initializing if exists.</param>
    public PlatformSupportPluginConfig(
        IOption[] defaultOptions,
        Quality defaultQuality,
        Format defaultFormat,
        int? defaultSearchResultsLimit,
        Sorting defaultSearchResultsSorting,
        bool defaultSearchResultsSortDescending,
        PlatformSupportPluginConfig? initialConfig = null)
    {
        // Set default values
        this.defaultOptions = defaultOptions;
        this.defaultQuality = defaultQuality;
        this.defaultFormat = defaultFormat;
        this.defaultSearchResultsLimit = defaultSearchResultsLimit;
        this.defaultSearchResultsSorting = defaultSearchResultsSorting;
        this.defaultSearchResultsSortDescending = defaultSearchResultsSortDescending;

        // No config passed, set config to default values
        if (initialConfig is null)
        {
            Options = [.. defaultOptions];
            foreach (IOption option in Options)
                option.PropertyChanged += OnItemPropertyChanged;

            Reset();
            return;
        }

        // Config passed, validate and set config to inital config values
        if (!initialConfig.ContainsAll(defaultOptions))
            throw new PluginConfigException(this, new("Passed initial config does not match additional options requiered for the plugin."));

        Options = initialConfig.Options.Select((option, i) => defaultOptions[i].Copy(option.Value)).ToArray();
        foreach (IOption option in Options)
            option.PropertyChanged += OnItemPropertyChanged;

        Quality = initialConfig.Quality;
        Format = initialConfig.Format;
        SearchResultsLimit = initialConfig.SearchResultsLimit;
        SearchResultsSorting = initialConfig.SearchResultsSorting;
        SearchResultsSortDescending = initialConfig.SearchResultsSortDescending;
    }


    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(((IOption)sender!).Name);


    /// <summary>
    /// Additional config options for the plugin.
    /// </summary>
    public IOption[] Options { get; } = default!;
    readonly IOption[] defaultOptions;

    /// <summary>
    /// The quality in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Quality quality = default!;
    readonly Quality defaultQuality;

    /// <summary>
    /// The format in which tracks get downloaded.
    /// </summary>
    [ObservableProperty]
    Format format = default!;
    readonly Format defaultFormat;

    /// <summary>
    /// The limit of search results to fetch.
    /// </summary>
    [ObservableProperty]
    int? searchResultsLimit = default!;
    readonly int? defaultSearchResultsLimit;

    /// <summary>
    /// The sorting of search results.
    /// </summary>
    [ObservableProperty]
    Sorting searchResultsSorting = default!;
    readonly Sorting defaultSearchResultsSorting;

    /// <summary>
    /// Weither search results are sorted descending or not.
    /// </summary>
    [ObservableProperty]
    bool searchResultsSortDescending = default!;
    readonly bool defaultSearchResultsSortDescending;


    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < Options.Length; i++)
            Options[i].Value = defaultOptions[i].Value;

        Quality = defaultQuality;
        Format = defaultFormat;
        SearchResultsLimit = defaultSearchResultsLimit;
        SearchResultsSorting = defaultSearchResultsSorting;
        SearchResultsSortDescending = defaultSearchResultsSortDescending;
    }
}