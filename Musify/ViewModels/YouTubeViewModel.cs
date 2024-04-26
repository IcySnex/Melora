using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Services;
using Musify.Views;
using SpotifyAPI.Web;
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

        SearchSorting = Config.YouTube.SearchSorting;

        logger.LogInformation("[YouTubeViewModel-.ctor] YouTubeViewModel has been initialized");
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableSortableRangeCollection<IVideo> SearchResults { get; } = [];

    public IList<object>? SelectedSearchResults { get; set; }

    public bool CanDownload => SelectedSearchResults is not null && SelectedSearchResults.Count > 0;


    [ObservableProperty]
    Sorting searchSorting;

    partial void OnSearchSortingChanged(
        Sorting value)
    {
        Config.YouTube.SearchSorting = value;
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
                SearchResults.OrderBy(track => track.Author.ChannelTitle);
                break;
            case Sorting.ArtistInv:
                SearchResults.OrderBy(track => track.Author.ChannelTitle, true);
                break;
            case Sorting.Duration:
                SearchResults.OrderBy(track => track.Duration);
                break;
            case Sorting.DurationInv:
                SearchResults.OrderBy(track => track.Duration, true);
                break;
        }

        logger.LogInformation("[YouTubeViewModel-OnSearchSortingChanged] Resorted search results: {sorting}", value);
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
        logger.LogInformation("[YouTubeViewModel-SearchAsync] Staring search for query on YouTube...");

        try
        {
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Preparing search...");
            progress.Report("Preparing search...");

            SearchResults.SkipForceRefresh = true;

            YouTubeSearchType type = YouTube.GetSearchType(Query, out string? id);
            switch (type)
            {
                case YouTubeSearchType.Video:
                    IVideo video = await youTube.SearchVideoAsync(id!, progress, cts.Token);

                    logger.LogInformation("[YouTubeViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    SearchResults.Add(video);
                    break;
                case YouTubeSearchType.Playlist:
                    IAsyncEnumerable<IVideo> playlistVideos = youTube.SearchPlaylistAsync(id!, progress, cts.Token).Take(Config.YouTube.SearchResultsLimit);

                    logger.LogInformation("[YouTubeViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(playlistVideos);
                    break;
                case YouTubeSearchType.Channel:
                    IAsyncEnumerable<IVideo> channelVideos = youTube.SearchChannelAsync(id!, progress, cts.Token).Take(Config.YouTube.SearchResultsLimit);

                    logger.LogInformation("[YouTubeViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(channelVideos);
                    break;
                case YouTubeSearchType.Query:
                    IAsyncEnumerable<IVideo> searchedVideos = youTube.SearchQueryAsync(id!, progress, cts.Token).Take(Config.YouTube.SearchResultsLimit);

                    logger.LogInformation("[YouTubeViewModel-SearchAsync] Updating search results...");
                    progress.Report("Updating search results...");

                    SearchResults.Clear();
                    await SearchResults.AddRangeAsync(searchedVideos);
                    break;
            }

            logger.LogInformation("[YouTubeViewModel-SearchAsync] Resorting search results...");
            progress.Report("Resorting search results...");

            SearchResults.SkipForceRefresh = false;
            OnSearchSortingChanged(SearchSorting);

            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Searched for query on YouTube: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            SearchResults.SkipForceRefresh = false;

            logger.LogInformation("[YouTubeViewModel-SearchAsync] Cancelled search for query on YouTube");
        }
        catch (Exception ex)
        {
            SearchResults.SkipForceRefresh = false;
            mainView.HideLoadingPopup();

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