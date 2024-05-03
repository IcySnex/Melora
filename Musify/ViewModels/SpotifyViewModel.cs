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
using System.ComponentModel;

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

        SearchResults = new()
        {
            KeySelector = Config.Spotify.ViewOptions.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Name,
                Sorting.Artist => track => track.Artists[0].Name,
                Sorting.Duration => track => track.DurationMs,
                _ => null
            },
            Descending = Config.Spotify.ViewOptions.Descending,
            Limit = Config.Spotify.ViewOptions.Limit
        };
        Config.Spotify.ViewOptions.PropertyChanged += OnViewOptionsPropertyChanged;

        logger.LogInformation("[SpotifyViewModel-.ctor] SpotifyViewModel has been initialized");
    }

    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableRangeCollection<FullTrack> SearchResults { get; }

    public IList<object>? SelectedSearchResults { get; set; }

    public bool CanDownload => SelectedSearchResults is not null && SelectedSearchResults.Count > 0;


    private async void OnViewOptionsPropertyChanged(
        object? _,
        PropertyChangedEventArgs e)
    {
        IProgress<string> progress = mainView.ShowLoadingPopup();
        progress.Report("Reordering search results");
        await Task.Delay(100);

        try
        {
            switch (e.PropertyName)
            {
                case "Sorting":
                    SearchResults.KeySelector = Config.Spotify.ViewOptions.Sorting switch
                    {
                        Sorting.Default => null,
                        Sorting.Title => track => track.Name,
                        Sorting.Artist => track => track.Artists[0].Name,
                        Sorting.Duration => track => track.DurationMs,
                        _ => null
                    };
                    break;
                case "Descending":
                    SearchResults.Descending = Config.Spotify.ViewOptions.Descending;
                    break;
                case "Limit":
                    SearchResults.Limit = Config.Spotify.ViewOptions.Limit;
                    break;
            }
        }
        finally
        {
            mainView.HideLoadingPopup();

            logger.LogInformation("[SpotifyViewModel-OnViewOptionsPropertyChanged] Reordered search results");
        }
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
        logger.LogInformation("[SpotifyViewModel-SearchAsync] Staring search on Spotify...");

        try
        {
            progress.Report("Preparing search...");

            SpotifySearchType type = Spotify.GetSearchType(Query, out string? id);
            switch (type)
            {
                case SpotifySearchType.Track:
                    progress.Report("Searching for track...");
                    FullTrack track = await spotify.SearchTrackAsync(id!, cts.Token);

                    SearchResults.Clear();
                    SearchResults.Add(track);
                    break;
                 case SpotifySearchType.Album:
                    progress.Report("Searching for album...");
                    (IAsyncEnumerable<FullTrack> albumTracks, int albumTracksCount) = await spotify.SearchAlbumAsync(id!, cts.Token);

                    Action<int, FullTrack> albumCallback = (int count, FullTrack track) =>
                        progress.Report($"Buffering tracks... [{count}/{albumTracksCount}]");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(albumTracks, albumCallback, cts.Token);
                    break;
                 case SpotifySearchType.Playlist:
                    progress.Report("Searching for playlist...");
                    (IAsyncEnumerable<FullTrack> playlistTracks, int playlistTracksCount) = await spotify.SearchPlaylistAsync(id!, cts.Token);

                    Action<int, FullTrack> playlistCallback = (int count, FullTrack track) =>
                        progress.Report($"Buffering tracks... [{count}/{playlistTracksCount}]");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(playlistTracks, playlistCallback, cts.Token);
                    break;
                case SpotifySearchType.Query:
                    progress.Report("Searching for query...");
                    IEnumerable<FullTrack> searchedTracks = await spotify.SearchQueryAsync(Query, cts.Token);

                    SearchResults.Clear();
                    SearchResults.AddRange(searchedTracks);
                    break;
            }

            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Searched on Spotify: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Cancelled search for query on Spotify");
        }
        catch (Exception ex)
        {
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