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

namespace Musify.ViewModels;

public partial class YouTubeViewModel : ObservableObject
{
    readonly ILogger<YouTubeViewModel> logger;
    readonly MainView mainView;
    readonly YouTube youTube;

    public Config Config { get; }

    public YouTubeViewModel(
        ILogger<YouTubeViewModel> logger,
        IOptions<Config> config,
        MainView mainView,
        YouTube youTube)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.Config = config.Value;
        this.youTube = youTube;

        SearchResults = new()
        {
            KeySelector = Config.YouTube.ViewOptions.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Title,
                Sorting.Artist => track => track.Author.ChannelTitle,
                Sorting.Duration => track => track.Duration ?? TimeSpan.FromMilliseconds(-1),
                _ => null
            },
            Descending = Config.YouTube.ViewOptions.Descending,
            Limit = Config.YouTube.ViewOptions.Limit
        };
        Config.YouTube.ViewOptions.PropertyChanged += OnViewOptionsPropertyChanged;

        logger.LogInformation("[YouTubeViewModel-.ctor] YouTubeViewModel has been initialized");
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableRangeCollection<IVideo> SearchResults { get; }

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
                    SearchResults.KeySelector = Config.YouTube.ViewOptions.Sorting switch
                    {
                        Sorting.Default => null,
                        Sorting.Title => video => video.Title,
                        Sorting.Artist => video => video.Author.ChannelTitle,
                        Sorting.Duration => video => video.Duration ?? TimeSpan.FromMilliseconds(-1),
                        _ => null
                    };
                    break;
                case "Descending":
                    SearchResults.Descending = Config.YouTube.ViewOptions.Descending;
                    break;
                case "Limit":
                    SearchResults.Limit = Config.YouTube.ViewOptions.Limit;
                    break;
            }
        }
        finally
        {
            mainView.HideLoadingPopup();

            logger.LogInformation("[YouTubeViewModel-OnViewOptionsPropertyChanged] Reordered search results");
        }
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            await mainView.AlertAsync("Your query can not be empty. Paste in a YouTube URL or type in a video title/channel name to search for videos.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[YouTubeViewModel-SearchAsync] Staring search on YouTube...");

        try
        {
            progress.Report("Preparing search...");

            YouTubeSearchType type = YouTube.GetSearchType(Query, out string? id);
            switch (type)
            {
                case YouTubeSearchType.Video:
                    progress.Report("Searching for video...");
                    IVideo video = await youTube.SearchVideoAsync(id!, cts.Token);

                    SearchResults.Clear();
                    SearchResults.Add(video);
                    break;
                case YouTubeSearchType.Playlist:
                    progress.Report("Searching for playlist...");
                    IAsyncEnumerable<IVideo> playlistVideos = youTube.SearchPlaylistAsync(id!, cts.Token);

                    Action<int, IVideo> playlistCallback = (int index, IVideo video) =>
                        progress.Report($"Buffering videos... [{index}]");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(playlistVideos, playlistCallback, cts.Token);
                    break;
                case YouTubeSearchType.Channel:
                    progress.Report("Searching for channel...");
                    IAsyncEnumerable<IVideo> channelVideos = youTube.SearchChannelAsync(id!, cts.Token);

                    Action<int, IVideo> channelCallback = (int index, IVideo video) =>
                        progress.Report($"Buffering videos... [{index}]");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(channelVideos, channelCallback, cts.Token);
                    break;
                case YouTubeSearchType.Query:
                    progress.Report("Searching for query...");
                    IAsyncEnumerable<IVideo> searchedVideos = youTube.SearchQueryAsync(Query, cts.Token).Take(100);

                    Action<int, IVideo> searchedCallback = (int index, IVideo video) =>
                        progress.Report($"Buffering videos... [{index}/100]");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(searchedVideos, searchedCallback, cts.Token);
                    break;
            }

            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Searched on YouTube: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Cancelled search for query on YouTube");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on YouTube.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeViewModel-SearchAsync] Failed to search for query on YouTube: {exception}", ex.Message);
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
        logger.LogInformation("[YouTubeViewModel-DownloadAsync] Staring move of selected videos to download queue...");

        try
        {
            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeViewModel-DownloadAsync] Moved selected videos to download queue");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeViewModel-DownloadAsync] Cancelled move of selected videos to download queue");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to move selected videos to download queue.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeViewModel-DownloadAsync] Failed to move of selected videos to download queue: {exception}", ex.Message);
        }
    }
}