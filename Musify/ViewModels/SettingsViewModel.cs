using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using Musify.Views;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Musify.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    readonly ILogger<SettingsViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        IOptions<Config> config,
        MainView mainView)
    {
        this.logger = logger;
        this.Config = config.Value;
        this.mainView = mainView;

        PathsDownloadLocation = Config.Paths.DownloadLocation;
        PathsFFMPEGLocation = Config.Paths.FFMPEGLocation;

        logger.LogInformation("[SettingsViewModel-.ctor] SettingsViewModel has been initialized");
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
    void CreateLoggerView() =>
        mainView.CreateLoggerView();
}