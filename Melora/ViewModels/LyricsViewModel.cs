using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeniusAPI;
using GeniusAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Melora.Enums;
using Melora.Helpers;
using Melora.Models;
using Melora.Plugins.Enums;
using Melora.Views;
using System.ComponentModel;

namespace Melora.ViewModels;

public partial class LyricsViewModel : ObservableObject
{
    public const string IconPathData = "M410.8 22.7c26.5 21.3 50.2 48.9 66.1 77 17.7 31.3 27.5 60.3 32.9 97.3 2 13.4 2.2 17.4 2.2 36.2 0 18.6-.2 23-2.1 35.7-4.6 31.3-12 55.4-25 81.8-21.3 43.5-51.3 77.7-92 105.1-39.6 26.6-85.6 42-133.9 44.9-29.1 1.7-64.7-2.5-91.6-10.7-8.2-2.6-22.3-7.5-23.2-8.1-.4-.3-2.9-1.4-5.5-2.3-13.5-5-38.4-19.3-55.2-31.8C63.6 433.1 41 410 26.6 390c-2.4-3.3-4.2-6.1-4-6.3s3.2 1.5 6.8 3.8c10.1 6.5 32.4 17.6 46 22.9C105 422 146.1 429 177.6 427.7c16-.7 24-1.5 37-3.8 60.8-11.2 107.2-35.6 149.2-78.8 19.7-20.3 32.3-37.8 45.5-63.2 29.6-57.3 37.3-120.9 22.2-185.6-5.7-24.7-19.5-57.8-32.7-78.2-2.4-3.7-4.2-6.8-4.2-7.2-.1-.4 9.7 6.5 16.2 11.8h0zM60.7 37.4h30.5 30.5v38.1c0 37.9 0 38.2-1.7 39.7-1 .8-4.7 5.2-8.3 9.7-28.2 34.9-41.4 76.7-38.1 120 1.6 22.1 7.7 45 16.4 62.1.9 1.8 1.5 3.3 1.4 3.5-.2.2-3.9-1.9-8.4-4.6-28-16.8-51-41.7-65.1-70.3-8.8-17.9-12.2-28.5-16-50.1-2.5-14.2-2.5-39.2.1-52.1 2.6-13.2 5.9-23.2 11.9-35.4 9.3-19.1 22.4-36.6 41-54.8l5.8-5.8h0zm220.1 0l36.2.2c0 .3 23.4 69.9 24 71.6.6 1.4-.3 1.5-11.6 1.5h-12.3l-.2 36c-.3 35.1-.3 36.2-2.2 41.8-8.3 25.2-29.1 42-54.3 44-18.2 1.5-35.8-5.5-48.5-19.1-7.6-8.1-14.1-20-15.1-27.8l-.4-3 6.4-2.1c17.9-5.9 33.3-22.4 38.7-41.2 1.7-5.9 1.8-8 2-52.9l.7-47.9c.2-.8 8.3-1.1 36.6-1.1h0z";


    readonly ILogger<LyricsViewModel> logger;
    readonly MainView mainView;
    readonly GeniusClient geniusClient;

    public Config Config { get; }

    public LyricsViewModel(
        ILogger<LyricsViewModel> logger,
        Config config,
        MainView mainView,
        GeniusClient geniusClient)
    {
        this.logger = logger;
        this.Config = config;
        this.mainView = mainView;
        this.geniusClient = geniusClient;

        SearchResults.KeySelector = Config.Lyrics.SearchResultsSorting switch
        {
            Sorting.Default => null,
            Sorting.Title => track => track.Title,
            Sorting.Artist => track => track.PrimaryArtist.Name,
            _ => null
        };
        SearchResults.Descending = Config.Lyrics.SearchResultsSortDescending;

        config.Lyrics.PropertyChanged += OnConfigPropertyChanged;

        logger.LogInformation("[LyricsViewModel-.ctor] LyricsViewModel has been initialized");
    }


    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "GeniusAccessToken":
                geniusClient.AccessToken = Config.Lyrics.GeniusAccessToken;
                break;
            case "SearchResultsSorting":
                SearchResults.KeySelector = Config.Lyrics.SearchResultsSorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => track => track.Title,
                    Sorting.Artist => track => track.PrimaryArtist.Name,
                    _ => null
                };
                break;
            case "SearchResultsSortDescending":
                SearchResults.Descending = Config.Lyrics.SearchResultsSortDescending;
                break;
        }
        logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<GeniusTrack> SearchResults { get; } = [];


    [ObservableProperty]
    GeniusTrack? selectedSearchResult;

    async partial void OnSelectedSearchResultChanged(
        GeniusTrack? value)
    {
        if (value is null)
            return;

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[LyricsViewModel-OnSelectedSearchResultChanged] Staring getting lyrics from Genius...");

        try
        {
            progress.Report("Fetching track lyrics...");
            string lyricsContent = await geniusClient.FetchLyricsAsync(value.Url, cts.Token) ?? throw new Exception("Returned lyrics from GeniusClient is null.");

            logger.LogInformation("[LyricsViewModel-OnSelectedSearchResultChanged] Creating LyricsInfoViewModel...");
            progress.Report("Creating lyrics info viewmodel...");

            LyricsInfoViewModel viewModel = App.Provider.GetRequiredService<LyricsInfoViewModel>();
            viewModel.Track = value;
            viewModel.Lyrics = lyricsContent;

            mainView.HideLoadingPopup();

            await mainView.AlertAsync(new LyricsInfoView(viewModel));
            SelectedSearchResult = null;

            logger.LogInformation("[LyricsViewModel-OnSelectedSearchResultChanged] Got lyrics from Genius: {title}-{artists}", value.Title, value.ArtistNames);

        }
        catch (OperationCanceledException)
        {
            SelectedSearchResult = null;

            logger.LogWarning("[LyricsViewModel-OnSelectedSearchResultChanged] Cancelled getting lyrics from Genius");
        }
        catch (Exception ex)
        {
            SelectedSearchResult = null;
            mainView.HideLoadingPopup();

            mainView.ShowNotification("Something went wrong!", "Failed to get lyrics from Genius.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[LyricsViewModel-OnSelectedSearchResultChanged] Failed to get lyrics from Genius: {exception}", ex.Message);
        }
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            mainView.ShowNotification("Warning!", "Query cannot be empty.", NotificationLevel.Warning, $"Type in a track title or artist name to search on Genius.");
            logger.LogWarning("[PlatformViewModel-SearchAsync] Tried to search for empty query.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[LyricsViewModel-SearchAsync] Staring search for query on Genius...");

        try
        {
            progress.Report("Searching for query...");
            IEnumerable<GeniusTrack> tracks = await geniusClient.SearchTracksAsync(Query, cts.Token);

            logger.LogInformation("[LyricsViewModel-SearchAsync] Updating search results...");
            progress.Report("Updating search results...");

            SearchResults.Clear();
            SearchResults.AddRange(tracks);

            mainView.HideLoadingPopup();
            logger.LogInformation("[LyricsViewModel-SearchAsync] Searched for query on Genius: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[LyricsViewModel-SearchAsync] Cancelled search for query on Genius");
        }
        catch (Exception ex)
        {
            mainView.HideLoadingPopup();

            mainView.ShowNotification("Something went wrong!", $"Failed to search on Genius.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[LyricsViewModel-SearchAsync] Failed to search for query on Genius: {exception}", ex.Message);
        }
    }
}