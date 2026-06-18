using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melora.Enums;
using Melora.Helpers;
using Melora.Models;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Services;
using Melora.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Numerics;
using Windows.System;

namespace Melora.ViewModels;

public partial class DownloadsViewModel : ObservableObject
{
    readonly ILogger<DownloadsViewModel> logger;
    readonly MainView mainView;
    readonly MediaEncoder encoder;

    public Config Config { get; }
    public PluginManager PluginManager { get; }

    public DownloadsViewModel(
        ILogger<DownloadsViewModel> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView,
        MediaEncoder encoder)
    {
        this.logger = logger;
        this.Config = config;
        this.PluginManager = pluginManager;
        this.mainView = mainView;
        this.encoder = encoder;

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
                ShowTracksFrom.Any(pair => download.PluginHash == pair.Key && pair.Value)
        };

        Config.Downloads.PropertyChanged += OnConfigPropertyChanged;
        Downloads.CollectionChanged += (_, _) => UpdatePositions();

        logger.LogInformation("[DownloadsViewModel-.ctor] DownloadsViewModel has been initialized");
    }


    void UpdatePositions()
    {
        for (int i = 0; i < Downloads.Count; i++)
            Downloads[i].Position = i + 1;
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
                logger.LogInformation("[DownloadsViewModel-OnConfigPropertyChanged] Reordered search results");
                break;
            case "SortDescending":
                Downloads.Descending = Config.Downloads.SortDescending;
                logger.LogInformation("[DownloadsViewModel-OnConfigPropertyChanged] Reordered search results");
                break;
        }
    }


    public ObservableRangeCollection<DownloadContainer> Downloads { get; }


    public Dictionary<int, bool> ShowTracksFrom { get; } = [];


    [RelayCommand]
    async Task ShowTrackInfoAsync(
        DownloadContainer download)
    {
        DownloadableTrackInfoViewModel viewModel = App.Provider.GetRequiredService<DownloadableTrackInfoViewModel>();
        viewModel.Track = download.Track;

        await mainView.AlertAsync(new DownloadableTrackInfoView(viewModel));
        logger.LogInformation("[DownloadsViewModel-ShowTrackInfoAsync] Showed download track info");
    }
    
    [RelayCommand]
    async Task ChangeTrackIdAsync(
        DownloadContainer download)
    {
        await mainView.AlertAsync(new DownloadableChangeIdView(download.Track));
        logger.LogInformation("[DownloadsViewModel-ShowTrackInfoAsync] Showed change track ID");
    }

    [RelayCommand]
    async Task OpenTrackSourceAsync(
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
        logger.LogInformation("[DownloadsViewModel-OnQueryChanged] Refreshed downloads");
    }


    [RelayCommand]
    async Task<Exception?> DownloadAsync(
        DownloadContainer download)
    {
        IProgress<TimeSpan> progress = new Progress<TimeSpan>(currentTime => download.Progress = (int)(currentTime / download.Track.Duration * 100));
        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Starting download of track");

        string? filePath = null;
        async Task TryDeleteFile()
        {
            try
            {
                await Task.Delay(750, CancellationToken.None);

                if (filePath is not null)
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MediaEncoder-OnCancellationTokenCanceled] Failed to kill processor and delete file: {exception}", ex.Message);
            }
        }

        try
        {
            PlatformSupportPlugin plugin = PluginManager.GetLoadedOrDefault<PlatformSupportPlugin>(download.PluginHash)
                ?? throw new Exception($"Could not get PlatformSupport plugin ({download.PluginHash}).", new("Please make sure the plugin is still loaded and was not removed."));
            MetadataPlugin metadataPlugin = PluginManager.GetLoadedOrDefault<MetadataPlugin>(Config.Downloads.SelectedMetadatePlugin)
                ?? throw new Exception($"Could not get Metadata plugin ({Config.Downloads.SelectedMetadatePlugin}).", new("Please make sure a plugin is loaded and selected (https://icysnex.github.io/Melora/guide/metadata.html#selecting-a-metadata-plugin)."));

            string fileName = Config.Paths.Filename
                .Replace("{title}", download.Track.Title)
                .Replace("{artists}", download.Track.Artists.Split(", ")[0])
                .Replace("{album}", download.Track.Album)
                .Replace("{release}", download.Track.ReleasedAt.ToString("MM-dd-yyyyy"))
                .Replace("{track}", download.Track.TrackNumber.ToString())
                .Replace("{tracks}", download.Track.TotalTracks.ToString())
                .Replace("{disc}", download.Track.DiscNumber.ToString())
                .Replace("{discs}", download.Track.TotalDiscs.ToString())
                .Replace("{pos}", (download.BatchPosition ?? download.Position).ToString())
                .Replace("{max}", (download.BatchTotal ?? Downloads.Count).ToString())
                .ToLegitFileName()
                .Trim('\\');
            filePath = Path.Combine(Config.Paths.DownloadLocation, Path.ChangeExtension(fileName, $".{plugin.Config.Format}"));

            if (File.Exists(filePath))
                switch (Config.Downloads.AlreadyExistsBehavior)
                {
                    case AlreadyExistsBehavior.Ask:
                        if (await mainView.AlertAsync("A track with the same file name already exists in the download location. Would you like to overwrite this file?", "Warning!", "No", "Yes") != ContentDialogResult.Primary)
                            return null;
                        break;
                    case AlreadyExistsBehavior.Skip:
                        mainView.ShowNotification("Warning!", $"Skipped download of track: {download.Track.Title}.", NotificationLevel.Warning, "A track with the same file name already exists in the download location and the 'Already Exists Behaviour' is set to skip.");
                        return null;
                    case AlreadyExistsBehavior.Overwrite:
                        break;
                }

            download.Progress = 0;
            download.IsProcessing = true;
            Stream stream = await plugin.GetStreamAsync(download.Track, download.CancellationSource.Token);

            download.IsProcessing = false;
            await encoder.WriteAsync(
                filePath,
                stream,
                plugin.Config.Quality,
                progress,
                download.CancellationSource.Token);

            download.IsProcessing = true;
            await metadataPlugin.WriteAsync(
                filePath,
                download.Track,
                download.CancellationSource.Token);

            Downloads.Remove(download);
            download.Dispose();

            mainView.ShowNotification("Success!", $"Finished downloading track: {download.Track.Title}.", NotificationLevel.Success);
            logger.LogInformation("[DownloadsViewModel-DownloadAsync] Finished download of track");
            return null;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[DownloadsViewModel-DownloadAsync] Cancelled download of track {trackTitle}", download.Track.Title);

            await TryDeleteFile();
            download.Reset();
            return null;
        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Something went wrong!", $"Failed to download track: {download.Track.Title}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[DownloadsViewModel-DownloadAsync] Failed to download track {trackTitle}: {exception}", download.Track.Title, ex.Message);

            await TryDeleteFile();
            download.Reset();
            return ex;
        }
    }

    [RelayCommand]
    void Remove(
        DownloadContainer download)
    {
        download.CancellationSource.Cancel();
        download.Dispose();
        Downloads.Remove(download);

        logger.LogInformation("[DownloadsViewModel-RemoveDownload] Removed track from downloads");
    }


    [RelayCommand(IncludeCancelCommand = true)]
    async Task DownloadAllAsync(
        CancellationToken cancellationToken)
    {
        if (Downloads.Count == 0)
        {
            await mainView.AlertAsync("There are currently no downloads.\nSearch for tracks on a plugin to start downloading or change the filter options.", "Something went wrong!");
            return;
        }

        logger.LogInformation("[DownloadsViewModel-DownloadAllAsync] Starting to download all tracks...");

        DownloadContainer[] queue = [.. Downloads];
        for (int i = 0; i < queue.Length; i++)
        {
            queue[i].BatchPosition = i + 1;
            queue[i].BatchTotal = queue.Length;
        }

        try
        {
            foreach (DownloadContainer download in queue)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (!download.IsIdle || download.IsDisposed)
                    continue;

                CancellationTokenRegistration registration = cancellationToken.Register(download.CancellationSource.Cancel);

                await DownloadCommand.ExecuteAsync(download);
                await registration.DisposeAsync();

                if (((Task<Exception?>?)DownloadCommand.ExecutionTask)?.Result is Exception ex)
                    switch (Config.Downloads.ErrorBehavior)
                    {
                        case ErrorBehavior.Ask:
                            if (await mainView.AlertAsync($"A track failed to download. Would you like to continue downloading the queue or stop?\n\n{ex.ToFormattedString()}", "Warning!", "Stop", "Continue") != ContentDialogResult.Primary)
                                return;
                            break;
                        case ErrorBehavior.Stop:
                            return;
                        case ErrorBehavior.Ignore:
                            continue;
                    }
            }
        }
        finally
        {
            foreach (DownloadContainer download in queue)
            {
                download.BatchPosition = null;
                download.BatchTotal = null;
            }
        }
        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Finished downloading all tracks");

        if (await mainView.AlertAsync("All tracks have finished to download.\nDo you want to open the download location in the file explorer?", "Queue finished!", "No", "Yes") == ContentDialogResult.Primary)
            await Launcher.LaunchFolderPathAsync(Config.Paths.DownloadLocation);
    }

    [RelayCommand]
    async Task RemoveAllAsync()
    {
        if (await mainView.AlertAsync("By clearing all your downloads your queue will be emptied and all running downloads will be stopped.", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        DownloadAllCommand.Cancel();
        Downloads.Clear();

        logger.LogInformation("[DownloadsViewModel-ClearAsync] Cleared all tracks from downloads");
    }
}