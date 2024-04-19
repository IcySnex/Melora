using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Helpers;
using Musify.Models;
using Musify.Views;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Musify.ViewModels;

public partial class LyricsViewModel : ObservableObject
{
    readonly ILogger<LyricsViewModel> logger;
    readonly MainView mainView;

    public LyricsViewModel(
        ILogger<LyricsViewModel> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        GenerateRandomSearchResults(10);

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


    readonly Random random = new();

    void GenerateRandomSearchResults(
        int count)
    {
        string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        for (int i = 0; i < count; i++)
        {
            string title = GenerateRandomString(random, 5 + random.Next(10));
            string artist = GenerateRandomString(random, 5 + random.Next(10));

            SearchResults.Add(new(title, artist, "song", "genius.com", $"https://cataas.com/cat"));
        }
    }


    public ObservableRangeCollection<LyricsSearchResult> SearchResults { get; } = [];


    [ObservableProperty]
    LyricsSearchResult? selectedSearchResult;

    async partial void OnSelectedSearchResultChanged(
        LyricsSearchResult? value)
    {
        if (value is null)
            return;

        await mainView.AlertAsync("lyrics");
        SelectedSearchResult = null;

        logger.LogInformation("[LyricsViewModel-OpenLyrics] Opened lyrics: {title}-{artists}", value.Title, value.Artists);
    }


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    async Task Search()
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
            progress.Report("Starting work...");
            await Task.Delay(1000, cts.Token);
            progress.Report("Doing work...");
            await Task.Delay(3000, cts.Token);
            progress.Report("Finishing work...");
            await Task.Delay(3000, cts.Token);

            mainView.HideLoadingPopup();
            logger.LogInformation("[LyricsViewModel-SearchAsync] Searched for query on Genius: {query}", Query);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("[LyricsViewModel-SearchAsync] Cancelled search for query on Genius");
        }
        catch (Exception ex)
        {
            await mainView.AlertAsync($"Failed to search for query on Genius.\nException: {ex.Message}", "Something went wrong.");
            logger.LogError("[LyricsViewModel-SearchAsync] Failed to search for query on Genius: {exception}", ex.Message);
        }
    }
}