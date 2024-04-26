using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Services;
using Musify.Views;
using SpotifyAPI.Web;

namespace Musify.ViewModels;

public partial class SpotifyViewModel : ObservableObject
{
    readonly ILogger<SpotifyViewModel> logger;
    readonly MainView mainView;
    readonly Spotify spotify;

    public Config Config { get; }

    public SpotifyViewModel(
        ILogger<SpotifyViewModel> logger,
        IOptions<Config> config,
        MainView mainView,
        Spotify spotify)
    {
        this.logger = logger;
        this.Config = config.Value;
        this.mainView = mainView;
        this.spotify = spotify;

        SearchSorting = Config.Spotify.SearchSorting;

        logger.LogInformation("[SpotifyViewModel-.ctor] SpotifyViewModel has been initialized");
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableSortableRangeCollection<FullTrack> SearchResults { get; } = [];

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
                SearchResults.OrderBy(track => track.Name);
                break;
            case Sorting.TitleInv:
                SearchResults.OrderBy(track => track.Name, true);
                break;
            case Sorting.Artist:
                SearchResults.OrderBy(track => track.Artists[0].Name);
                break;
            case Sorting.ArtistInv:
                SearchResults.OrderBy(track => track.Artists[0].Name, true);
                break;
            case Sorting.Duration:
                SearchResults.OrderBy(track => track.DurationMs);
                break;
            case Sorting.DurationInv:
                SearchResults.OrderBy(track => track.DurationMs, true);
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
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Preparing search...");
            progress.Report("Preparing search...");

            SearchResults.SkipForceRefresh = true;

            SpotifySearchType type = Spotify.GetSearchType(Query, out string? id);
            switch (type)
            {
                case SpotifySearchType.Track:
                    FullTrack track = await spotify.SearchTrackAsync(id!, progress, cts.Token);

                    logger.LogInformation("[SpotifyViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    SearchResults.Add(track);
                    break;
                 case SpotifySearchType.Album:
                    IAsyncEnumerable<FullTrack> albumTracks = (await spotify.SearchAlbumAsync(id!, progress, cts.Token)).Take(Config.Spotify.SearchResultsLimit);

                    logger.LogInformation("[SpotifyViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(albumTracks, cts.Token);
                    break;
                 case SpotifySearchType.Playlist:
                    IAsyncEnumerable<FullTrack> playlistTracks = (await spotify.SearchPlaylistAsync(id!, progress, cts.Token)).Take(Config.Spotify.SearchResultsLimit);

                    logger.LogInformation("[SpotifyViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(playlistTracks, cts.Token);
                    break;
                case SpotifySearchType.Query:
                    IEnumerable<FullTrack> searchedTracks = (await spotify.SearchQueryAsync(Query, progress, cts.Token)).Take(Config.Spotify.SearchResultsLimit);

                    logger.LogInformation("[SpotifyViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    SearchResults.AddRange(searchedTracks);
                    break;
            }

            logger.LogInformation("[SpotifyViewModel-SearchAsync] Resorting search results...");
            progress.Report("Resorting search results...");

            SearchResults.SkipForceRefresh = false;
            OnSearchSortingChanged(SearchSorting);

            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Searched for query on Spotify: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            SearchResults.SkipForceRefresh = false;

            logger.LogInformation("[SpotifyViewModel-SearchAsync] Cancelled search for query on Spotify");
        }
        catch (Exception ex)
        {
            SearchResults.SkipForceRefresh = false;
            mainView.HideLoadingPopup();

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