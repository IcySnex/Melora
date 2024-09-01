using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melora.Enums;
using Melora.Helpers;
using Melora.Models;
using Melora.Plugins.Abstract;
using Melora.Services;
using Melora.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Melora.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    readonly ILogger<SettingsViewModel> logger;
    readonly MainView mainView;
    readonly UpdateManager updateManager;

    public Config Config { get; }
    public PluginManager PluginManager { get; }

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        Config config,
        PluginManager pluginManager,
        UpdateManager updateManager,
        MainView mainView)
    {
        this.logger = logger;
        this.updateManager = updateManager;
        this.PluginManager = pluginManager;
        this.Config = config;
        this.mainView = mainView;

        PathsDownloadLocation = Config.Paths.DownloadLocation;
        PathsFFmpegLocation = Config.Paths.FFmpegLocation;

        logger.LogInformation("[SettingsViewModel-.ctor] SettingsViewModel has been initialized");
    }


    [ObservableProperty]
    string pathsDownloadLocation;

    [ObservableProperty]
    string pathsFFmpegLocation;

    partial void OnPathsDownloadLocationChanged(
        string? oldValue,
        string newValue)
    {
        if (!Directory.Exists(newValue))
        {
            mainView.ShowNotification("Something went wrong!", "Could not set download location.", NotificationLevel.Error, "It looks like this directory doesnt exist. Please make sure you have created the folder.");

            PathsDownloadLocation = oldValue ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            return;
        }

        Config.Paths.DownloadLocation = newValue;
        logger.LogInformation("[SettingsViewModel-OnPathsDownloadLocationChanged] Validated & updated Config.Paths.DownloadLocation");
    }

    partial void OnPathsFFmpegLocationChanged(
        string? oldValue,
        string newValue)
    {
        if (!File.Exists(newValue))
        {
            mainView.ShowNotification("Something went wrong!", "Could not set FFmpeg path.", NotificationLevel.Error, "It looks like this file does not exist. Please make sure you have downloaded the FFmpeg binary to this path.");

            if (oldValue is not null && File.Exists(oldValue))
                PathsFFmpegLocation = oldValue;
            return;
        }

        Config.Paths.FFmpegLocation = newValue;
        logger.LogInformation("[SettingsViewModel-OnPathsFFmpegLocationChanged] Validated & updated Config.Paths.PathsFFmpegLocation");
    }

    [RelayCommand]
    async Task SelectPathsDownloadLocationAsync()
    {
        FolderPicker picker = new()
        {
            CommitButtonText = "Set download location",
            SuggestedStartLocation = PickerLocationId.MusicLibrary,
            SettingsIdentifier = "Set download location"
        };
        mainView.Initialize(picker);

        StorageFolder? folder = await picker.PickSingleFolderAsync();
        logger.LogInformation("[SettingsViewModel-SelectPathsDownloadLocationAsync] Folder picker was shown");

        if (folder is null)
            return;
        PathsDownloadLocation = folder.Path;
    }

    [RelayCommand]
    async Task SelectPathsFFmpegLocationAsync()
    {
        FileOpenPicker picker = new()
        {
            CommitButtonText = "Set FFmpeg executable",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            SettingsIdentifier = "Set FFmpeg executable"
        };
        picker.FileTypeFilter.Add(".exe");
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSingleFileAsync();
        logger.LogInformation("[SettingsViewModel-SelectPathsFFmpegLocationAsync] File open picker was shown");

        if (file is null)
            return;
        PathsFFmpegLocation = file.Path;
    }


    public async Task<bool> TryGetUpdatesAsync()
    {
        try
        {
            Release? release = await updateManager.GetLatestReleaseAsync();
            if (release is null || release.Version <= UpdateManager.Version)
            {
                logger.LogInformation("[SettingsViewModel-TryGetUpdatesAsync] Newest version is already installed");
                return true;
            }

            UpdateInfoView view = new(release);
            if (await mainView.AlertAsync(view, "New Update Available!", "Not now", "Install") != ContentDialogResult.Primary)
            {
                logger.LogInformation("[SettingsViewModel-TryGetUpdatesAsync] Found new update, but got ignored");
                return false;
            }

            CancellationTokenSource cts = new();
            IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);

            await updateManager.InstallReleaseAsync(release, progress, cts.Token);

            logger.LogInformation("[SettingsViewModel-TryGetUpdatesAsync] Installed update");
            return false;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[PlatformViewModel-TryGetUpdatesAsync] Cancelled getting updates");
            return false;
        }
        catch (Exception ex)
        {
            mainView.HideLoadingPopup();

            mainView.ShowNotification("Warning!", $"Failed to get any updates.", NotificationLevel.Warning, ex.ToFormattedString());
            logger.LogWarning(ex, "[SettingsViewModel-TryGetUpdatesAsync] Failed get any updates: {exception}", ex.Message);
            return false;
        }
    }

    [RelayCommand]
    async Task CheckForUpdatesAsync()
    {
        if (await TryGetUpdatesAsync())
            mainView.ShowNotification("No Updates Found!", "You already have installed the latest updates.", NotificationLevel.Success);
    }


    [RelayCommand]
    static async Task OpenPluginsDirectoryAsync() =>
        await Launcher.LaunchFolderPathAsync(PluginManager.PluginsDirectory);


    [RelayCommand]
    async Task ShowAboutAsync()
    {
        AboutView view = new();
        await mainView.AlertAsync(view);
    }

    [RelayCommand]
    void CreateLoggerView() =>
        mainView.CreateLoggerView();

    public async Task ResetPluginConfigAsync(
        IPlugin plugin)
    {
        if (await mainView.AlertAsync("Resetting the plugin config will delete ALL settings and preferences.", "Are you sure?", "Cancel", "Yes") != ContentDialogResult.Primary)
            return;

        plugin.Config.Reset();

        mainView.ShowNotification("Success!", $"Reset plugins config: {plugin.Name}", NotificationLevel.Success);
        logger.LogInformation("[SettingsViewModel-ResetPluginConfig] Reset plugins config to default: {pluginName}", plugin.Name);
    }

    [RelayCommand]
    async Task ResetConfig()
    {
        if (await mainView.AlertAsync("Resetting the config will delete ALL your settings and preferences. Your plugin configs won't be reset.", "Are you sure?", "Cancel", "Yes") != ContentDialogResult.Primary)
            return;

        Config.Reset();

        mainView.ShowNotification("Success!", $"Reset config", NotificationLevel.Success);
        logger.LogInformation("[SettingsViewModel-ResetConfig] Reset config");
    }
}