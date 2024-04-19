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

public partial class YouTubeViewModel : ObservableObject
{
    readonly ILogger<YouTubeViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }

    public YouTubeViewModel(
        ILogger<YouTubeViewModel> logger,
        IOptions<Config> config,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.Config = config.Value;

        GenerateRandomVideos(50);

        SearchSorting = Config.YouTube.SearchSorting;

        logger.LogInformation("[YouTubeViewModel-.ctor] YouTubeViewModel has been initialized");
    }


    readonly Random random = new();

    void GenerateRandomVideos(
        int count)
    {
        string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        for (int i = 0; i < count; i++)
        {
            string title = GenerateRandomString(random, 5 + random.Next(10));
            string artist = GenerateRandomString(random, 5 + random.Next(10));
            TimeSpan duration = TimeSpan.FromSeconds(random.Next(60, 600));

            Videos.Add(new(title, artist, duration, $"https://cataas.com/cat"));
        }
    }


    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableSortableRangeCollection<Track> Videos { get; } = [];

    public IList<object>? SelectedVideos { get; set; }

    public bool CanDownloadVideos => SelectedVideos is not null && SelectedVideos.Count > 0;


    [ObservableProperty]
    Sorting searchSorting;

    partial void OnSearchSortingChanged(
        Sorting value)
    {
        Config.YouTube.SearchSorting = value;
        switch (value)
        {
            case Sorting.Default:
                Videos.OrderbyDefault();
                break;
            case Sorting.DefaultInv:
                Videos.OrderbyDefault(true);
                break;
            case Sorting.Title:
                Videos.OrderBy(track => track.Title);
                break;
            case Sorting.TitleInv:
                Videos.OrderBy(track => track.Title, true);
                break;
            case Sorting.Artist:
                Videos.OrderBy(track => track.Artist);
                break;
            case Sorting.ArtistInv:
                Videos.OrderBy(track => track.Artist, true);
                break;
            case Sorting.Duration:
                Videos.OrderBy(track => track.Duration);
                break;
            case Sorting.DurationInv:
                Videos.OrderBy(track => track.Duration, true);
                break;
        }

        logger.LogInformation("[YouTubeViewModel-OnSearchSortingChanged] Resorted searched tracks: {sorting}", value);
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            await mainView.AlertAsync("Your query can not be empty. Please type in a video title or channel to start searching for videos.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[YouTubeViewModel-SearchAsync] Staring search for query on YouTube...");

        try
        {
            progress.Report("Starting work...");
            await Task.Delay(1000, cts.Token);
            progress.Report("Doing work...");
            await Task.Delay(3000, cts.Token);
            progress.Report("Finishing work...");
            await Task.Delay(3000, cts.Token);

            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Searched for query on YouTube: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeViewModel-SearchAsync] Cancelled search for query on YouTube");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on YouTube.\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeViewModel-SearchAsync] Failed to search for query on YouTube: {exception}", ex.Message);
        }

        await Task.Delay(1000);
    }


    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownloadVideos)
        {
            await mainView.AlertAsync("Please select at least one video to start downloading.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[YouTubeViewModel-DownloadAsync] Staring move of selected videos to download queue...");

        try
        {
            progress.Report("Starting work...");
            await Task.Delay(1000, cts.Token);
            progress.Report("Doing work...");
            await Task.Delay(3000, cts.Token);
            progress.Report("Finishing work...");
            await Task.Delay(3000, cts.Token);

            mainView.HideLoadingPopup();
            logger.LogInformation("[YouTubeViewModel-DownloadAsync] Moved selected videos to download queue");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[YouTubeViewModel-DownloadAsync] Cancelled move of selected videos to download queue");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to move selected videos to download queue.\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[YouTubeViewModel-DownloadAsync] Failed to move of selected videos to download queue: {exception}", ex.Message);
        }
    }
}