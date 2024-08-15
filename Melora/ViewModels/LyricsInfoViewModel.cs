using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeniusAPI.Models;
using Melora.Enums;
using Melora.Helpers;
using Melora.Views;
using Microsoft.Extensions.Logging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Melora.ViewModels;

public partial class LyricsInfoViewModel : ObservableObject
{
    readonly ILogger<LyricsInfoViewModel> logger;
    readonly MainView mainView;

    readonly HttpClient client = new();

    public LyricsInfoViewModel(
        ILogger<LyricsInfoViewModel> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        logger.LogInformation("[LyricsInfoViewModel-.ctor] LyricsInfoViewModel has been initialized");
    }


    public GeniusTrack Track { get; set; } = default!;

    public string Lyrics { get; set; } = string.Empty;


    [RelayCommand]
    async Task SaveArtworkAsync()
    {
        if (Track.ArtworklUrl is null || Track.ArtworklUrl.StartsWith("file:///"))
        {
            mainView.ShowNotification("Warning!", "Can't save artwork.", NotificationLevel.Warning, "This track has no artwork set or its source is a local file.");
            logger.LogWarning("[LyricsInfoViewModel-SaveArtworkAsync] Track artwork url is null or a file. Could not save artwork.");
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
        logger.LogInformation("[LyricsInfoViewModel-SaveArtworkAsync] File save picker was shown");

        if (file is null)
            return;

        try
        {
            Stream artwork = await client.GetStreamAsync(Track.ArtworklUrl);
            logger.LogInformation("[LyricsInfoViewModel-SaveArtworkAsync] Saved stream for arwork");

            using Stream fileStream = await file.OpenStreamForWriteAsync();
            await artwork.CopyToAsync(fileStream);

            mainView.ShowNotification("Success!", "Saved artwork to file.", NotificationLevel.Success);
            logger.LogInformation("[LyricsInfoViewModel-SaveArtworkAsync] Saved artwork to file");

        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Something went wrong!", "Failed to save artwork.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[LyricsInfoViewModel-SaveArtworkAsync] Failed to save artwork: {exception}", ex.Message);
        }
    }


    [RelayCommand]
    void CopyLyricsToClipboard()
    {
        DataPackage data = new();
        data.SetText(Lyrics);

        Clipboard.SetContent(data);

        mainView.ShowNotification("Success!", "Copied lyrics to clipboard.", NotificationLevel.Success);
        logger.LogInformation("[LyricsInfoViewModel-CopyLyricsToClipboard] Copied lyrics to clipboard");
    }
}