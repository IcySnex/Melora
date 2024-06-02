using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Models;
using Musify.ViewModels;
using Windows.System;

namespace Musify.DataTemplates;

public partial class DownloadTemplate : ResourceDictionary
{
    public DownloadTemplate() =>
        InitializeComponent();


    DownloadsViewModel viewModel = default!;

    void OnDownloadClick(object sender, RoutedEventArgs _)
    {
    }

    void OnTrackInfoClick(object sender, RoutedEventArgs _)
    {
    }

    async void OnOpenSourceClick(object sender, RoutedEventArgs _)
    {
        Track track = (Track)((MenuFlyoutItem)sender).DataContext;

        await Launcher.LaunchUriAsync(new(track.Url));
    }

    void OnRemoveClick(object sender, RoutedEventArgs _)
    {
        Track track = (Track)((MenuFlyoutItem)sender).DataContext;
        viewModel ??= App.Provider.GetRequiredService<DownloadsViewModel>();

        viewModel.Remove(track);
    }
}