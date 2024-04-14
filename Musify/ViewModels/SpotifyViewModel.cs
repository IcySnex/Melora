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

        GenerateRandomTracks(50);

        SearchSorting = Config.Spotify.SearchSorting;

        logger.LogInformation("[SpotifyViewModel-.ctor] SpotifyViewModel has been initialized");
    }


    readonly Random random = new();

    void GenerateRandomTracks(
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

            Tracks.Add(new(title, artist, duration, $"https://cataas.com/cat"));
        }
    }


    public ObservableSortableRangeCollection<Track> Tracks { get; } = [];

    public IList<object>? SelectedTracks { get; set; }

    public void OnTracksSelectionChanged(object _, SelectionChangedEventArgs _1) =>
        OnPropertyChanged(nameof(CanDownloadTracks));

    public bool CanDownloadTracks => SelectedTracks is not null && SelectedTracks.Count > 0;


    [ObservableProperty]
    Sorting searchSorting;

    partial void OnSearchSortingChanged(
        Sorting value)
    {
        Config.Spotify.SearchSorting = value;
        switch (value)
        {
            case Sorting.Default:
                Tracks.OrderbyDefault();
                break;
            case Sorting.DefaultInv:
                Tracks.OrderbyDefault(true);
                break;
            case Sorting.Title:
                Tracks.OrderBy(track => track.Title);
                break;
            case Sorting.TitleInv:
                Tracks.OrderBy(track => track.Title, true);
                break;
            case Sorting.Artist:
                Tracks.OrderBy(track => track.Artist);
                break;
            case Sorting.ArtistInv:
                Tracks.OrderBy(track => track.Artist, true);
                break;
            case Sorting.Duration:
                Tracks.OrderBy(track => track.Duration);
                break;
            case Sorting.DurationInv:
                Tracks.OrderBy(track => track.Duration, true);
                break;
        }

        logger.LogInformation("[SpotifyViewModel-OnSearchSortingChanged] Resorted searched tracks: {sorting}", value);
    }


    [RelayCommand]
    async Task SearchAsync(
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await mainView.AlertAsync("Your query can not be empty. Please type in a track title or artist to start searching for tracks.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[SpotifyViewModel-SearchAsync] Staring search for query on Spotify...");

        try
        {
            progress.Report("Starting work...");
            await Task.Delay(1000, cts.Token);
            progress.Report("Doing work...");
            await Task.Delay(3000, cts.Token);
            progress.Report("Finishing work...");
            await Task.Delay(3000, cts.Token);

            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Searched for query on Spotify: {query}", query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[SpotifyViewModel-SearchAsync] Cancelled search for query on Spotify");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on Spotify.\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[SpotifyViewModel-SearchAsync] Failed to search for query on Spotify: {exception}", ex.Message);
        }
    }

    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownloadTracks)
        {
            await mainView.AlertAsync("Please select at least one track to start downloading.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[SpotifyViewModel-DownloadAsync] Staring move of selected tracks to download queue...");

        try
        {
            progress.Report("Starting work...");
            await Task.Delay(1000, cts.Token);
            progress.Report("Doing work...");
            await Task.Delay(3000, cts.Token);
            progress.Report("Finishing work...");
            await Task.Delay(3000, cts.Token);

            mainView.HideLoadingPopup();
            logger.LogInformation("[SpotifyViewModel-DownloadAsync] Moved selected tracks to download queue");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[SpotifyViewModel-DownloadAsync] Cancelled move of selected tracks to download queue");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to move selected tracks to download queue.\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[SpotifyViewModel-DownloadAsync] Failed to move of selected tracks to download queue: {exception}", ex.Message);
        }
    }
}