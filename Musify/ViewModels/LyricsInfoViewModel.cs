using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Musify.Views;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using Musify.Enums;
using Musify.Helpers;
using GeniusAPI.Models;

namespace Musify.ViewModels;

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
    async Task DownloadArtworkAsync()
    {
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
        logger.LogInformation("[LyricsInfoViewModel-DownloadArtworkAsync] File save picker was shown");

        if (file is null)
            return;

        try
        {
            Stream artwork = await client.GetStreamAsync(Track.ArtworklUrl);
            logger.LogInformation("[LyricsInfoViewModel-DownloadArtworkAsync] Downloaded stream for arwork");

            using Stream fileStream = await file.OpenStreamForWriteAsync();
            await artwork.CopyToAsync(fileStream);

            mainView.ShowNotification("Success!", "Downloaded artwork to file.", NotificationLevel.Success);
            logger.LogInformation("[LyricsInfoViewModel-DownloadArtworkAsync] Downloaded artwork to file");

        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Something went wrong!", "Failed to download artwork.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError("[LyricsInfoViewModel-DownloadArtworkAsync] Failed to download artwork: {exception}", ex.Message);
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