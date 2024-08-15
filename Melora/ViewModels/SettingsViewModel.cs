using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melora.Enums;
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

    public Config Config { get; }
    public PluginManager PluginManager { get; }

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView)
    {
        this.logger = logger;
        this.PluginManager = pluginManager;
        this.Config = config;
        this.mainView = mainView;

        PathsDownloadLocation = Config.Paths.DownloadLocation;
        PathsFFMPEGLocation = Config.Paths.FFMPEGLocation;

        logger.LogInformation("[SettingsViewModel-.ctor] SettingsViewModel has been initialized");
    }


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


    [ObservableProperty]
    string pathsDownloadLocation;

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


    [ObservableProperty]
    string pathsFFMPEGLocation;

    partial void OnPathsFFMPEGLocationChanged(
        string? oldValue,
        string newValue)
    {
        if (!File.Exists(newValue))
        {
            mainView.ShowNotification("Something went wrong!", "Could not set FFMPEG path.", NotificationLevel.Error, "It looks like this file does not exist. Please make sure you have downloaded the FFMEPG binary to this path.");

            PathsFFMPEGLocation = oldValue ?? "FFMPEG.exe";
            return;
        }

        Config.Paths.FFMPEGLocation = newValue;
        logger.LogInformation("[SettingsViewModel-OnPathsFFMPEGLocationChanged] Validated & updated Config.Paths.FFMPEGLocation");
    }

    [RelayCommand]
    async Task SelectPathsFFMPEGLocationAsync()
    {
        FileOpenPicker picker = new()
        {
            CommitButtonText = "Set FFMPEG executable",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            SettingsIdentifier = "Set FFMPEG executable"
        };
        picker.FileTypeFilter.Add(".exe");
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSingleFileAsync();
        logger.LogInformation("[SettingsViewModel-SelectPathsFFMPEGLocationAsync] File open picker was shown");

        if (file is null)
            return;
        PathsFFMPEGLocation = file.Path;
    }


    [RelayCommand]
    async Task OpenPluginsDirectoryAsync() =>
        await Launcher.LaunchFolderPathAsync(PluginManager.PluginsDirectory);


    [RelayCommand]
    void CreateLoggerView() =>
        mainView.CreateLoggerView();
}