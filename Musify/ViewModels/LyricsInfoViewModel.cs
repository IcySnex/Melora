using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Musify.Models;
using Musify.Views;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

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


    public LyricsTrack Track { get; set; } = default!;

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
            logger.LogInformation("[LyricsInfoViewModel-DownloadArtworkAsync] Saved artwork to file");

        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to download artwork.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[LyricsInfoViewModel-DownloadArtworkAsync] Failed to download artwork: {exception}", ex.Message);
        }
    }


    [RelayCommand]
    async Task ShowOnGeniusAsync()
    {
        await Launcher.LaunchUriAsync(new(Track.Url));
        logger.LogInformation("[LyricsInfoViewModel-OpenBrowserAsync] Lyrics was shown on Genius");
    }

    [RelayCommand]
    async Task CopyLyricsToClipboardAsync()
    {
        if (await mainView.AlertAsync($"Do you want to copy the lyrics to your clipboard?", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        DataPackage data = new();
        data.SetText(Lyrics);

        Clipboard.SetContent(data);
        logger.LogInformation("[LyricsInfoViewModel-CopyLyricsToClipboardAsync] Clipboard was set to lyrics");
    }
}