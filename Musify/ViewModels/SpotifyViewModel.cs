using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Views;
using System.Diagnostics;

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

    public bool CanDownloadTracks => SelectedTracks is not null && SelectedTracks.Count > 0;

    public void OnTracksSelectionChanged(object _, SelectionChangedEventArgs _1) =>
        OnPropertyChanged(nameof(CanDownloadTracks));


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
            await mainView.AlertAsync("Your query can not be empty. Please type in a title or artist to start searching for tracks.", "Something went wrong.");
            return;
        }

        await Task.Delay(1000);
        logger.LogInformation("[SpotifyViewModel-SearchAsync] Searched for query on Spotify: {query}", query);
    }

    [RelayCommand]
    async Task DownloadAsync()
    {
        if (!CanDownloadTracks)
        {
            await mainView.AlertAsync("Please select at least one track to start downloading.", "Something went wrong.");
            return;
        }

        await Task.Delay(1000);
        logger.LogInformation("[SpotifyViewModel-DownloadAsync] Moved selected tracks to download queue");
    }
}