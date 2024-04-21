using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Views;

namespace Musify.ViewModels;

public partial class SpotifyViewModel : ObservableObject
{
    readonly ILogger<SpotifyViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }

    public SpotifyViewModel(
        ILogger<SpotifyViewModel> logger,
        IOptions<Config> config,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.Config = config.Value;

        SearchSorting = Config.Spotify.SearchSorting;

        logger.LogInformation("[SpotifyViewModel-.ctor] SpotifyViewModel has been initialized");
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableSortableRangeCollection<Track> SearchResults { get; } = [];

    public IList<object>? SelectedSearchResults { get; set; }

    public bool CanDownload => SelectedSearchResults is not null && SelectedSearchResults.Count > 0;



    [ObservableProperty]
    Sorting searchSorting;

    partial void OnSearchSortingChanged(
        Sorting value)
    {
        Config.Spotify.SearchSorting = value;
        switch (value)
        {
            case Sorting.Default:
                SearchResults.OrderbyDefault();
                break;
            case Sorting.DefaultInv:
                SearchResults.OrderbyDefault(true);
                break;
            case Sorting.Title:
                SearchResults.OrderBy(track => track.Title);
                break;
            case Sorting.TitleInv:
                SearchResults.OrderBy(track => track.Title, true);
                break;
            case Sorting.Artist:
                SearchResults.OrderBy(track => track.Artist);
                break;
            case Sorting.ArtistInv:
                SearchResults.OrderBy(track => track.Artist, true);
                break;
            case Sorting.Duration:
                SearchResults.OrderBy(track => track.Duration);
                break;
            case Sorting.DurationInv:
                SearchResults.OrderBy(track => track.Duration, true);
                break;
        }

        logger.LogInformation("[SpotifyViewModel-OnSearchSortingChanged] Resorted search results: {sorting}", value);
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            await mainView.AlertAsync("Your query can not be empty. Paste in a Spotify URL or type in a track title/artist name to search for tracks.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[SpotifyViewModel-SearchAsync] Staring search for query on Spotify...");

        try
        {
            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Searched for query on Spotify: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Cancelled search for query on Spotify");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on Spotify.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[SpotifyViewModel-SearchAsync] Failed to search for query on Spotify: {exception}", ex.Message);
        }
    }


    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownload)
        {
            await mainView.AlertAsync("Please select at least one track to start downloading.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[SpotifyViewModel-DownloadAsync] Staring move of selected tracks to download queue...");

        try
        {
            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-DownloadAsync] Moved selected tracks to download queue");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[SpotifyViewModel-DownloadAsync] Cancelled move of selected tracks to download queue");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to move selected tracks to download queue.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[SpotifyViewModel-DownloadAsync] Failed to move of selected tracks to download queue: {exception}", ex.Message);
        }
    }
}