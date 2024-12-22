using Melora.Models;
using Melora.Plugins.Abstract;
using Melora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Views;

public sealed partial class DownloadsView : Page
{
    public DownloadsViewModel ViewModel { get; } = App.Provider.GetRequiredService<DownloadsViewModel>();

    public DownloadsView()
    {
        InitializeComponent();

        ViewModel.PluginManager.Subscribe<PlatformSupportPlugin>(
            plugin =>
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
                    ViewModel.ShowTracksFrom[pluginHash] = ((ToggleMenuFlyoutItem)s).IsChecked;
                    ViewModel.Downloads.Refresh();
                };

                ToolTipService.SetToolTip(pluginFlyoutItem, $"Show tracks from {plugin.Name}");
                ShowTracksFromFlyout.Items.Add(pluginFlyoutItem);

                ViewModel.ShowTracksFrom[pluginHash] = true;
            },
            plugin =>
            {
                ToggleMenuFlyoutItem? pluginFlyoutItem = ShowTracksFromFlyout.Items.OfType<ToggleMenuFlyoutItem>().FirstOrDefault(item => item.Text == plugin.Name);
                ShowTracksFromFlyout.Items.Remove(pluginFlyoutItem);

                ViewModel.ShowTracksFrom.Remove(plugin.GetHashCode());
            });
    }


    void OnCancelClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        download.CancellationSource.Cancel();
    }

    void OnDownloadClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        ViewModel.DownloadCommand.Execute(download);
    }

    void OnTrackInfoClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        ViewModel.ShowTrackInfoCommand.Execute(download);
    }
    
    void OnChangeTrackIdClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        ViewModel.ChangeTrackIdCommand.Execute(download);
    }

    void OnRemoveClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        ViewModel.RemoveCommand.Execute(download);
    }

    void OnOpenSourceClick(object sender, RoutedEventArgs _)
    {
        DownloadContainer download = (DownloadContainer)((MenuFlyoutItem)sender).DataContext;
        ViewModel.OpenTrackSourceCommand.Execute(download);
    }
}