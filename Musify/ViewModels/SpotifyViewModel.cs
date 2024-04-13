using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        SelectedTracks.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsDownloadBarVisible));

        GenerateRandomTracks(50);

        logger.LogInformation("[SpotifyViewModel-.ctor] SpotifyViewModel has been initialized");

    }


    public ObservableRangeCollection<Track> Tracks { get; } = [];

    public ObservableRangeCollection<Track> SelectedTracks { get; } = [];


    public bool IsDownloadBarVisible => SelectedTracks.Count > 0;


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
        if (SelectedTracks.Count < 1)
        {
            await mainView.AlertAsync("Please select at least one track to start downloading.", "Something went wrong.");
            return;
        }

        await Task.Delay(1000);
        logger.LogInformation("[SpotifyViewModel-DownloadAsync] Moved selected tracks to download queue: {count}", SelectedTracks.Count);
    }
}