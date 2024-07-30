using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.Views;
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
                Sorting.Title => download => download.Track.Title,
                Sorting.Artist => download => download.Track.Artists,
                Sorting.Duration => download => download.Track.Duration,
                _ => null
            },
            Descending = Config.Downloads.SortDescending,
            Filter = download =>
                (
                    download.Track.Title.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ||
                    download.Track.Artists.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ||
                    (download.Track.Album?.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ?? false)
                ) &&
                ShowTracksFrom.Any(pair => download.Track.Plugin.Name == pair.Key && pair.Value)
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
                    Sorting.Title => download => download.Track.Title,
                    Sorting.Artist => download => download.Track.Artists,
                    Sorting.Duration => download => download.Track.Duration,
                    _ => null
                };
                break;
            case "SortDescending":
                Downloads.Descending = Config.Downloads.SortDescending;
                break;
        }
        logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<DownloadContainer> Downloads { get; }


    public Dictionary<string, bool> ShowTracksFrom { get; } = [];


    public async Task ShowTrackInfoAsync(
        DownloadContainer download)
    {
        DownloadableTrackInfoViewModel viewModel = App.Provider.GetRequiredService<DownloadableTrackInfoViewModel>();
        viewModel.Track = download.Track;

        await mainView.AlertAsync(new DownloadableTrackInfoView(viewModel));
        logger.LogInformation("[DownloadsViewModel-ShowTrackInfoAsync] Showed download track info");
    }

    public async Task OpenTrackSourceAsync(
        DownloadContainer download)
    {
        await Launcher.LaunchUriAsync(new(download.Track.Url));
        logger.LogInformation("[DownloadsViewModel-OpenTrackSourceAsync] Browser was opened with track source url");
    }


    [ObservableProperty]
    string query = string.Empty;

    partial void OnQueryChanged(
        string value)
    {
        Downloads.Refresh();
        logger.LogInformation("[DownloadsViewModel-OnPropertyChanged] Refreshed downloads");
    }


    public async Task DownloadAsync(
        DownloadContainer download)
    {
        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Starting download of track");
        try
        {
            download.IsProcessing = true;
            Stream stream = await download.Track.Plugin.GetStreamAsync(download.Track);

            download.IsProcessing = false;
            for (int i = 0; i < 100; i += 10) // Simulate download...
            {
                await Task.Delay(500);
                download.Progress = i;
            }

            download.IsProcessing = true;
            await Task.Delay(1000); // Simulate after download progress, e.g. writing metadata...

            Remove(download);
            logger.LogInformation("[DownloadsViewModel-DownloadAsync] Finished download of track");
        }
        catch (OperationCanceledException)
        {
            download.IsProcessing = false;
            download.Progress = 0;

            logger.LogInformation("[DownloadsViewModel-DownloadAsync] Cancelled download of track {trackTitle}", download.Track.Title);
        }
        catch (Exception ex)
        {
            download.IsProcessing = false;
            download.Progress = 0;

            mainView.ShowNotification("Something went wrong!", $"Failed to download track {download.Track.Title}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError("[DownloadsViewModel-DownloadAsync] Failed to download track {trackTitle}: {exception}", download.Track.Title, ex.Message);
        }
    }

    public void Remove(
        DownloadContainer download)
    {
        Downloads.Remove(download);
        logger.LogInformation("[DownloadsViewModel-RemoveDownload] Removed track from downloads");
    }


    [RelayCommand]
    async Task DownloadAllAsync()
    {
        await Task.Delay(1000);

        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Downloaded all tracks");
    }

    [RelayCommand]
    async Task RemoveAllAsync()
    {
        if (await mainView.AlertAsync("By clearing all your downloads your queue will be emptied and all running downloads will be stopped.", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        Downloads.Clear();
        logger.LogInformation("[DownloadsViewModel-ClearAsync] Cleared all tracks from downloads");
    }
}