using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Services;
using Musify.Views;
using System.ComponentModel;
using YoutubeExplode.Videos;
using YouTubeMusicAPI.Models;

namespace Musify.ViewModels;

public partial class YouTubeMusicViewModel : ObservableObject
{
    readonly ILogger<YouTubeMusicViewModel> logger;
    readonly MainView mainView;
    readonly Navigation navigation;
    readonly YouTubeMusic youTubeMusic;
    readonly DownloadsViewModel downloadsViewModel;

    public Config Config { get; }

    public YouTubeMusicViewModel(
        ILogger<YouTubeMusicViewModel> logger,
        IOptions<Config> config,
        MainView mainView,
        Navigation navigation,
        YouTubeMusic youTubeMusic,
        DownloadsViewModel downloadsViewModel)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.Config = config.Value;
        this.navigation = navigation;
        this.youTubeMusic = youTubeMusic;
        this.downloadsViewModel = downloadsViewModel;

        SearchResults = new()
        {
            KeySelector = Config.YouTubeMusic.ViewOptions.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Name,
                Sorting.Artist => track => track.Artists[0].Name,
                Sorting.Duration => track => track.Duration,
                _ => null
            },
            Descending = Config.YouTubeMusic.ViewOptions.Descending,
            Limit = Config.YouTubeMusic.ViewOptions.Limit
        };
        Config.YouTubeMusic.ViewOptions.PropertyChanged += OnViewOptionsPropertyChanged;

        logger.LogInformation("[YouTubeMusicViewModel-.ctor] YouTubeMusicViewModel has been initialized");
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableRangeCollection<Song> SearchResults { get; }

    public IList<object>? SelectedSearchResults { get; set; }

    public bool CanDownload => SelectedSearchResults is not null && SelectedSearchResults.Count > 0;


    private void OnViewOptionsPropertyChanged(
        object? _,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                SearchResults.KeySelector = Config.YouTubeMusic.ViewOptions.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => track => track.Name,
                    Sorting.Artist => track => track.Artists[0].Name,
                    Sorting.Duration => track => track.Duration,
                    _ => null
                };
                logger.LogInformation("[YouTubeMusicViewModel-OnViewOptionsPropertyChanged] Reordered search results");
                break;
            case "Descending":
                SearchResults.Descending = Config.YouTubeMusic.ViewOptions.Descending;
                logger.LogInformation("[YouTubeMusicViewModel-OnViewOptionsPropertyChanged] Reordered search results");
                break;
            case "Limit":
                SearchResults.Limit = Config.YouTubeMusic.ViewOptions.Limit;
                logger.LogInformation("[YouTubeMusicViewModel-OnViewOptionsPropertyChanged] Reordered search results");
                break;
        }
    }


    string? album = null;

    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            await mainView.AlertAsync("Your query can not be empty. Paste in a YouTube Music URL or type in a song title/artist name to search for songs.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[YouTubeMusicViewModel-SearchAsync] Staring search on YouTube Music...");

        try
        {
            progress.Report("Preparing search...");

            YouTubeMusicSearchType type = YouTubeMusic.GetSearchType(Query, out string? id);
            switch (type)
            {
                case YouTubeMusicSearchType.Song:
                    progress.Report("Searching for song...");

                    SearchResults.Clear();
                    break;
                case YouTubeMusicSearchType.Album:
                    progress.Report("Searching for album...");

                    SearchResults.Clear();
                    break;
                case YouTubeMusicSearchType.CommunityPlaylist:
                    progress.Report("Searching for community playlist...");

                    SearchResults.Clear();
                    break;
                case YouTubeMusicSearchType.Channel:
                    progress.Report("Searching for channel...");

                    SearchResults.Clear();
                    break;
                case YouTubeMusicSearchType.Query:
                    progress.Report("Searching for query...");
                    IEnumerable<Song> searchedSongs = await youTubeMusic.SearchQueryAsync(Query, cts.Token);

                    SearchResults.Clear();
                    SearchResults.AddRange(searchedSongs);
                    break;
            }

            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeMusicViewModel-SearchAsync] Searched on YouTube Music: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeMusicViewModel-SearchAsync] Cancelled search for query on YouTube Music");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on YouTube Music.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeMusicViewModel-SearchAsync] Failed to search for query on YouTube Music: {exception}", ex.Message);
        }
    }


    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownload)
        {
            await mainView.AlertAsync("Please select at least one video to start downloading.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[YouTubeMusicViewModel-DownloadAsync] Staring move of selected songs to download queue...");

        try
        {
            IAsyncEnumerable<Track> tracks = youTubeMusic.ConvertAsync(SelectedSearchResults!.Cast<Song>(), album);


            navigation.Navigate("Downloads");
            navigation.SetCurrentIndex(7);

            Action<int, Track> callback = (int count, Track track) =>
                progress.Report($"Preparing downloads... [{count}/{SelectedSearchResults?.Count}]");

            await downloadsViewModel.AddAsync(tracks, callback, cts.Token);

            await Task.Delay(100);
            mainView.HideLoadingPopup();

            logger.LogInformation("[YouTubeMusicViewModel-DownloadAsync] Moved selected songs to download queue");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeMusicViewModel-DownloadAsync] Cancelled move of selected songs to download queue");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to move selected songs to download queue.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeMusicViewModel-DownloadAsync] Failed to move of selected songs to download queue: {exception}", ex.Message);
        }
    }
}