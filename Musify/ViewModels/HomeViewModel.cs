using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Services;
using Musify.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Musify.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    readonly ILogger<HomeViewModel> logger;
    readonly MainView mainView;
    readonly Navigation navigation;

    public HomeViewModel(
        ILogger<HomeViewModel> logger,
        MainView mainView,
        Navigation navigation)
    {
        this.logger = logger;
        this.mainView = mainView;
        this.navigation = navigation;

        logger.LogInformation("[HomeViewModel-.ctor] HomeViewModel has been initialized");
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


    [ObservableProperty]
    string query = string.Empty;

    [RelayCommand]
    void Search(
        string source)
    {
        int pageIndex = source switch
        {
            "Spotify" => 3,
            "YouTube" => 4,
            "Lyrics" => 5,
            _ => 0
        };

        navigation.Navigate(source, Query);
        navigation.SetCurrentIndex(pageIndex);
    }
}