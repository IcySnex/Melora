using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Musify.Enums;
using Musify.Helpers;
using Musify.Plugins.Models;
using Musify.Views;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Musify.ViewModels;

public partial class DownloadableTrackInfoViewModel : ObservableObject
{
    readonly ILogger<DownloadableTrackInfoViewModel> logger;
    readonly MainView mainView;

    readonly HttpClient client = new();

    public DownloadableTrackInfoViewModel(
        ILogger<DownloadableTrackInfoViewModel> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        logger.LogInformation("[DownloadableTrackInfoViewModel-.ctor] DownloadableTrackInfoViewModel has been initialized");
    }


    public DownloadableTrack Track { get; set; } = default!;


    [RelayCommand]
    async Task SaveArtworkAsync()
    {
        if (Track.ArtworkUrl is null)
        {
            mainView.ShowNotification("Warning!", "Can't save artwork.", NotificationLevel.Warning, "This track has no artwork set.");
            logger.LogWarning("[DownloadableTrackInfoViewModel-SaveArtworkAsync] Track artwork url is null. Could not save artwork.");
            return;
        }
        Uri source = new(Track.ArtworkUrl);
        if (source.IsFile)
        {
            mainView.ShowNotification("Warning!", "Can't save artwork.", NotificationLevel.Warning, "The artwork source is a file which can not be saved.");
            logger.LogWarning("[DownloadableTrackInfoViewModel-SaveArtworkAsync] Track artwork url is a file. Could not save artwork.");
            return;
        }

        FileSavePicker picker = new()
        {
            CommitButtonText = "Save artwork",
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SettingsIdentifier = "Save artwork",
            SuggestedFileName = "Artwork"
        };
        picker.FileTypeChoices.Add("Image", [".png"]);
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSaveFileAsync();
        logger.LogInformation("[DownloadableTrackInfoViewModel-SaveArtworkAsync] File save picker was shown");

        if (file is null)
            return;

        try
        {
            Stream artwork = await client.GetStreamAsync(source);
            logger.LogInformation("[DownloadableTrackInfoViewModel-SaveArtworkAsync] Saved stream for arwork");

            using Stream fileStream = await file.OpenStreamForWriteAsync();
            await artwork.CopyToAsync(fileStream);

            mainView.ShowNotification("Success!", "Saved artwork to file.", NotificationLevel.Success);
            logger.LogInformation("[DownloadableTrackInfoViewModel-SaveArtworkAsync] Saved artwork to file");

        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Something went wrong!", "Failed to save artwork.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[DownloadableTrackInfoViewModel-SaveArtworkAsync] Failed to save artwork: {exception}", ex.Message);
        }
    }

    [RelayCommand]
    async Task ChangeArtworkAsync()
    {
        FileOpenPicker picker = new()
        {
            CommitButtonText = "Change artwork",
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SettingsIdentifier = "Change artwork",
        };
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg ");
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSingleFileAsync();
        logger.LogInformation("[DownloadableTrackInfoViewModel-ChangeArtworkAsync] File open picker was shown");

        if (file is null)
            return;

        Track.ArtworkUrl = file.Path;
        logger.LogInformation("[DownloadableTrackInfoViewModel-ChangeArtworkAsync] Changed track artwork");
    }

}