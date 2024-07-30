using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Plugins.Models;
using Musify.Services;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class DownloadsView : Page
{
    readonly DownloadsViewModel viewModel = App.Provider.GetRequiredService<DownloadsViewModel>();

    public DownloadsView()
    {
        InitializeComponent();

        App.Provider.GetRequiredService<PluginManager<PlatformSupportPlugin>>().LoadedPlugins.CollectionChanged += (s, e) =>
        {
            if (e.NewItems is not null)
                foreach (PlatformSupportPlugin plugin in e.NewItems.Cast<PlatformSupportPlugin>())
                {
                    ToggleMenuFlyoutItem pluginFlyoutItem = new()
                    {
                        IsChecked = true,
                        Text = plugin.Name,
                    };
                    pluginFlyoutItem.Click += (s, e) =>
                    {
                        viewModel.ShowTracksFrom[plugin.Name] = ((ToggleMenuFlyoutItem)s).IsChecked;
                        viewModel.Downloads.Refresh();
                    };

                    ToolTipService.SetToolTip(pluginFlyoutItem, $"Show tracks from {plugin.Name}");
                    ShowTracksFromFlyout.Items.Add(pluginFlyoutItem);

                    viewModel.ShowTracksFrom[plugin.Name] = true;
                }

            if (e.OldItems is not null)
                foreach (PlatformSupportPlugin plugin in e.OldItems.Cast<PlatformSupportPlugin>())
                {
                    ToggleMenuFlyoutItem? pluginFlyoutItem = ShowTracksFromFlyout.Items.OfType<ToggleMenuFlyoutItem>().FirstOrDefault(item => item.Text == plugin.Name);
                    ShowTracksFromFlyout.Items.Remove(pluginFlyoutItem);
                }
        };
    }


    async void OnDownloadClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        await viewModel.DownloadAsync(download);
    }

    async void OnTrackInfoClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        await viewModel.ShowTrackInfoAsync(download);
    }

    async void OnOpenSourceClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        await viewModel.OpenTrackSourceAsync(download);
    }

    void OnRemoveClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        viewModel.Remove(download);
    }
}