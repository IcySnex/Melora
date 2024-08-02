using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.Services;
using Musify.Views;
using System.ComponentModel;

namespace Musify.ViewModels;

public partial class PlatformViewModel : ObservableObject
{
    readonly ILogger<PlatformViewModel> logger;
    readonly MainView mainView;
    readonly Navigation navigation;
    readonly DownloadsViewModel downloadsViewModel;

    public PlatformViewModel(
        ILogger<PlatformViewModel> logger,
        MainView mainView,
        Navigation navigation,
        DownloadsViewModel downloadsViewModel)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.navigation = navigation;
        this.downloadsViewModel = downloadsViewModel;

        logger.LogInformation("[PlatformViewModel-.ctor] PlatformViewModel has been initialized");
    }


    PlatformSupportPlugin plugin = default!;

    public PlatformSupportPlugin Plugin
    {
        get => plugin;
        set
        {
            value.Config.PropertyChanged -= OnConfigPropertyChanged;


            SearchResults.KeySelector = value.Config.SearchResultsSorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Title,
                Sorting.Artist => track => track.Artists,
                Sorting.Duration => track => track.Duration,
                _ => null
            };
            SearchResults.Descending = value.Config.SearchResultsSortDescending;

            value.Config.PropertyChanged += OnConfigPropertyChanged;

            plugin = value;
        }
    }


    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "SearchResultsSorting":
                SearchResults.KeySelector = Plugin.Config.SearchResultsSorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => track => track.Title,
                    Sorting.Artist => track => track.Artists,
                    Sorting.Duration => track => track.Duration,
                    _ => null
                };
                break;
            case "SearchResultsSortDescending":
                SearchResults.Descending = Plugin.Config.SearchResultsSortDescending;
                break;
        }
        logger.LogInformation("[PlatformViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<SearchResult> SearchResults { get; } = [];

    public IList<object> SelectedSearchResults { get; set; } = default!;


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            mainView.ShowNotification("Warning!", "Query cannot be empty.", NotificationLevel.Warning, $"Paste in a URL or type in a query to search on {Plugin.Name}.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[PlatformViewModel-SearchAsync] Staring search on {pluginName}...", Plugin.Name);
        try
        {
            SearchResults.Clear();

            IEnumerable<SearchResult> results = await plugin.SearchAsync(Query, progress, cts.Token);
            SearchResults.AddRange(results);

            mainView.HideLoadingPopup();
            logger.LogInformation("[PlatformViewModel-SearchAsync] Searched on {pluginName}: {query}", Plugin.Name, Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[PlatformViewModel-SearchAsync] Cancelled search on {pluginName}", Plugin.Name);
        }
        catch (Exception ex)
        {
            mainView.HideLoadingPopup();

            mainView.ShowNotification("Something went wrong!", $"Failed to search on {Plugin.Name}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError("[PlatformViewModel-SearchAsync] Failed to search on {pluginName}: {exception}", Plugin.Name, ex.Message);
        }
    }


    [RelayCommand]
    async Task PrepareDownloadsAsync()
    {
        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[PlatformViewModel-PrepareDownloadAsync] Staring preparing downloads on {pluginName}...", Plugin.Name);
        try
        {
            IEnumerable<DownloadableTrack> results = await plugin.PrepareDownloadsAsync(SelectedSearchResults.Cast<SearchResult>(), progress, cts.Token);
            downloadsViewModel.Downloads.AddRange(results.Select(d => new DownloadContainer(d)));

            mainView.HideLoadingPopup();
            navigation.SetCurrentItem("Downloads");
            logger.LogInformation("[PlatformViewModel-PrepareDownloadAsync] Prepared downloads on {pluginName}: {query}", Plugin.Name, Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[PlatformViewModel-PrepareDownloadAsync] Cancelled prepare for downloads on {pluginName}", Plugin.Name);
        }
        catch (Exception ex)
        {
            mainView.HideLoadingPopup();

            mainView.ShowNotification("Something went wrong!", $"Failed to prepare downloads on {Plugin.Name}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError("[PlatformViewModel-PrepareDownloadAsync] Failed to prepare downloads on {pluginName}: {exception}", Plugin.Name, ex.Message);
        }
    }
}