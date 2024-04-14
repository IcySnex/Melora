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


    public ObservableSortableRangeCollection<Track> Videos { get; } = [];

    public IList<object>? SelectedVideos { get; set; }

    public void OnVideosSelectionChanged(object _, SelectionChangedEventArgs _1) =>
        OnPropertyChanged(nameof(CanDownloadVideos));

    public bool CanDownloadVideos => SelectedVideos is not null && SelectedVideos.Count > 0;


    [ObservableProperty]
    Sorting searchSorting;

    partial void OnSearchSortingChanged(
        Sorting value)
    {
        Config.Spotify.SearchSorting = value;
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


    [RelayCommand]
    async Task SearchAsync(
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await mainView.AlertAsync("Your query can not be empty. Please type in a video title or channel to start searching for videos.", "Something went wrong.");
            return;
        }

        await Task.Delay(1000);
        logger.LogInformation("[YouTubeViewModel-SearchAsync] Searched for query on YouTube: {query}", query);
    }

    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownloadVideos)
        {
            await mainView.AlertAsync("Please select at least one video to start downloading.", "Something went wrong.");
            return;
        }

        await Task.Delay(1000);
        logger.LogInformation("[YouTubeViewModel-DownloadAsync] Moved selected videos to download queue");
    }
}