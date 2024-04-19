using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Helpers;
using Musify.Models;
using Musify.Services;
using Musify.Views;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Musify.ViewModels;

public partial class LyricsViewModel : ObservableObject
{
    readonly ILogger<LyricsViewModel> logger;
    readonly MainView mainView;
    readonly Lyrics lyrics;

    public LyricsViewModel(
        ILogger<LyricsViewModel> logger,
        MainView mainView,
        Lyrics lyrics)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.lyrics = lyrics;

        logger.LogInformation("[LyricsViewModel-.ctor] LyricsViewModel has been initialized");
    }


    [RelayCommand]
    async Task OpenBrowserAsync(
        string url)
    {
        if (await mainView.AlertAsync($"Do you want to open your default browser with the url '{url}'?", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        await Launcher.LaunchUriAsync(new(url));
        logger.LogInformation("[HomeViewModel-OpenBrowserAsync] Browser was opened with url: {url}", url);
    }

    [RelayCommand]
    async Task CopyToClipboardAsync(
        string text)
    {
        if (await mainView.AlertAsync($"Do you want to copy the text '{text}' to your clipboard?", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        DataPackage data = new();
        data.SetText(text);

        Clipboard.SetContent(data);
        logger.LogInformation("[HomeViewModel-ShareAsync] Clipboard was set to text: {text}", text);
    }


    public ObservableRangeCollection<LyricsHit> SearchResults { get; } = [];

    [ObservableProperty]
    LyricsHit? selectedSearchResult;

    async partial void OnSelectedSearchResultChanged(
        LyricsHit? value)
    {
        if (value is null)
            return;

        await mainView.AlertAsync("lyrics");
        SelectedSearchResult = null;

        logger.LogInformation("[LyricsViewModel-OpenLyrics] Opened lyrics: {title}-{artists}", value.Track.Title, value.Track.ArtistNames);
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            await mainView.AlertAsync("Your query can not be empty. Please type in a track title or artist to start searching for lyrics.", "Something went wrong.");
            return;
        }

        CancellationTokenSource cts = new();
        IProgress<string> progress = mainView.ShowLoadingPopup(cts.Cancel);
        logger.LogInformation("[LyricsViewModel-SearchAsync] Staring search for query on Genius...");

        try
        {
            LyricsHit[] hits = await lyrics.SearchAsync(Query, progress, cts.Token);

            logger.LogInformation("[LyricsViewModel-SearchAsync] Updating search results...");
            progress.Report("Updating search results...");

            SearchResults.Clear();
            SearchResults.AddRange(hits);

            mainView.HideLoadingPopup();
            logger.LogInformation("[LyricsViewModel-SearchAsync] Searched for query on Genius: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[LyricsViewModel-SearchAsync] Cancelled search for query on Genius");
        }
        catch (Exception ex)
        {
            mainView.HideLoadingPopup();

            await mainView.AlertAsync($"Failed to search for query on Genius.\n\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[LyricsViewModel-SearchAsync] Failed to search for query on Genius: {exception}", ex.Message);
        }
    }
}