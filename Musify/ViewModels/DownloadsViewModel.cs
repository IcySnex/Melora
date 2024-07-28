using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.Views;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Windows.System;

namespace Musify.ViewModels;

public partial class DownloadsViewModel : ObservableObject
{
    readonly ILogger<DownloadsViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }

    public DownloadsViewModel(
        ILogger<DownloadsViewModel> logger,
        Config config,
        MainView mainView)
    {
        this.logger = logger;
        this.Config = config;
        this.mainView = mainView;

        Downloads = new()
        {
            KeySelector = Config.Downloads.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => track => track.Title,
                Sorting.Artist => track => track.Artists,
                Sorting.Duration => track => track.Duration,
                _ => null
            },
            Descending = Config.Downloads.SortDescending,
            Filter = track =>
                (track.Title.Contains(Query, StringComparison.InvariantCultureIgnoreCase) || track.Artists.Contains(Query, StringComparison.InvariantCultureIgnoreCase)) &&
                ShowTracksFrom.Any(pair => track.Plugin.Name == pair.Key && pair.Value)
        };

        Config.Downloads.PropertyChanged += OnConfigPropertyChanged;

        logger.LogInformation("[DownloadsViewModel-.ctor] DownloadsViewModel has been initialized");
    }


    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                Downloads.KeySelector = Config.Downloads.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => track => track.Title,
                    Sorting.Artist => track => track.Artists,
                    Sorting.Duration => track => track.Duration,
                    _ => null
                };
                break;
            case "SortDescending":
                Downloads.Descending = Config.Downloads.SortDescending;
                break;
        }
        logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<DownloadableTrack> Downloads { get; }


    public Dictionary<string, bool> ShowTracksFrom { get; } = [];


    public async Task StartDownloadAsync(
        DownloadableTrack download)
    {
        await Task.Delay(1000);
        logger.LogInformation("[DownloadsViewModel-StartDownloadAsync] Started download of track");
    }

    public async Task ShowDownloadInfoAsync(
        DownloadableTrack download)
    {
        DownloadableTrackInfoViewModel viewModel = App.Provider.GetRequiredService<DownloadableTrackInfoViewModel>();
        viewModel.Track = download;

        await mainView.AlertAsync(new DownloadableTrackInfoView(viewModel));
        logger.LogInformation("[DownloadsViewModel-ShowDownloadInfo] Showed download track info");
    }

    public async Task OpenDownloadSourceAsync(
        DownloadableTrack download)
    {
        await Launcher.LaunchUriAsync(new(download.Url));
        logger.LogInformation("[DownloadsViewModel-OpenDownloadSourceAsync] Browser was opened with track source url");
    }
    
    public void RemoveDownload(
        DownloadableTrack download)
    {
        Downloads.Remove(download);
        logger.LogInformation("[DownloadsViewModel-RemoveDownload] Removed track from downloads");
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