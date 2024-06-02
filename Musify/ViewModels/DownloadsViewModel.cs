using AngleSharp.Dom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Views;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using System.ComponentModel;

namespace Musify.ViewModels;

public partial class DownloadsViewModel : ObservableObject
{
    readonly ILogger<DownloadsViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }

    public DownloadsViewModel(
        ILogger<DownloadsViewModel> logger,
        IOptions<Config> config,
        MainView mainView)
    {
        this.logger = logger;
        this.Config = config.Value;
        this.mainView = mainView;

        Downloads = new()
        {
            KeySelector = Config.Downloads.ViewOptions.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Title,
                Sorting.Artist => track => track.Artist,
                Sorting.Duration => track => track.Duration,
                _ => null
            },
            Descending = Config.Downloads.ViewOptions.Descending
        };
        Config.Downloads.PropertyChanged += OnPropertyChanged;
        Config.Downloads.ViewOptions.PropertyChanged += OnViewOptionsPropertyChanged;

        Downloads.Filter = track =>
            (track.Title.Contains(Query, StringComparison.InvariantCultureIgnoreCase) || track.Artist.Contains(Query, StringComparison.InvariantCultureIgnoreCase)) &&
            (Config.Downloads.ShowSpotifyTracks || track.Source != Source.Spotify) &&
            (Config.Downloads.ShowYouTubeTracks || track.Source != Source.YouTube);

        logger.LogInformation("[DownloadsViewModel-.ctor] DownloadsViewModel has been initialized");
    }

    [RelayCommand]
    void ForceUpdateProperty(
        string propertyName) =>
        OnPropertyChanged(propertyName);


    public ObservableRangeCollection<Track> Downloads { get; }


    private void OnPropertyChanged(
        object? _,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "ShowSpotifyTracks":
            case "ShowYouTubeTracks":
                Downloads.Refresh();
                logger.LogInformation("[DownloadsViewModel-OnPropertyChanged] Refreshed downloads");
                break;
        }
    }

    private void OnViewOptionsPropertyChanged(
        object? _,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                Downloads.KeySelector = Config.Downloads.ViewOptions.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => track => track.Title,
                    Sorting.Artist => track => track.Artist,
                    Sorting.Duration => track => track.Duration,
                    _ => null
                };
                logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered downloads");
                break;
            case "Descending":
                Downloads.Descending = Config.Downloads.ViewOptions.Descending;
                logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered downloads");
                break;
        }
    }


    public async Task AddAsync(
        IAsyncEnumerable<Track> downloads,
        Action<int, Track>? callback = null,
        CancellationToken cancellationToken = default)
    {
        await Downloads.AddRangeAsync(downloads, callback, cancellationToken);
        logger.LogInformation("[DownloadsViewModel-AddAsync] Added tracks to downloads");
    }

    public void Remove(
        Track track)
    {
        Downloads.Remove(track);
        logger.LogInformation("[DownloadsViewModel-Remove] Removed track from downloads");
    }


    [RelayCommand]
    async Task ClearAsync()
    {
        if (await mainView.AlertAsync("By clearing all your downloads your queue will be emptied and all running downloads will be stopped.", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        Downloads.Clear();
        logger.LogInformation("[DownloadsViewModel-ClearAsync] Cleared all tracks from downloads");
    }

    [RelayCommand]
    async Task DownloadAsync()
    {
        await Task.Delay(1000);

        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Downloaded all tracks");
    }


    [ObservableProperty]
    string query = string.Empty;

    partial void OnQueryChanged(
        string value)
    {
        Downloads.Refresh();
        logger.LogInformation("[DownloadsViewModel-OnPropertyChanged] Refreshed downloads");
    }

}