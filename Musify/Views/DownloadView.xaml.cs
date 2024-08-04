using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Models;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class DownloadsView : Page
{
    readonly DownloadsViewModel viewModel = App.Provider.GetRequiredService<DownloadsViewModel>();

    public DownloadsView()
    {
        InitializeComponent();

        viewModel.PluginManager.PluginLoaded += (s, plugin) =>
        {
            int pluginHash = plugin.GetHashCode();

            ToggleMenuFlyoutItem pluginFlyoutItem = new()
            {
                IsChecked = true,
                Tag = pluginHash,
                Text = plugin.Name,
            };
            pluginFlyoutItem.Click += (s, e) =>
            {
                viewModel.ShowTracksFrom[pluginHash] = ((ToggleMenuFlyoutItem)s).IsChecked;
                viewModel.Downloads.Refresh();
            };

            ToolTipService.SetToolTip(pluginFlyoutItem, $"Show tracks from {plugin.Name}");
            ShowTracksFromFlyout.Items.Add(pluginFlyoutItem);

            viewModel.ShowTracksFrom[pluginHash] = true;
        };
        viewModel.PluginManager.PluginUnloaded += (s, plugin) =>
        {
            ToggleMenuFlyoutItem? pluginFlyoutItem = ShowTracksFromFlyout.Items.OfType<ToggleMenuFlyoutItem>().FirstOrDefault(item => item.Text == plugin.Name);
            ShowTracksFromFlyout.Items.Remove(pluginFlyoutItem);

            viewModel.ShowTracksFrom.Remove(plugin.GetHashCode());
        };
    }


    void OnCancelClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        download.CancellationSource.Cancel();
    }

    void OnDownloadClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        viewModel.DownloadCommand.Execute(download);
    }

    void OnTrackInfoClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        viewModel.ShowTrackInfoCommand.Execute(download);
    }

    void OnRemoveClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        viewModel.RemoveCommand.Execute(download);
    }

    void OnOpenSourceClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        viewModel.OpenTrackSourceCommand.Execute(download);
    }
}