using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Plugins.Abstract;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class SettingsView : Page
{
    readonly SettingsViewModel viewModel = App.Provider.GetRequiredService<SettingsViewModel>();

    public SettingsView()
    {
        InitializeComponent();

        viewModel.PluginManager.Subscribe<PlatformSupportPlugin>(
            plugin =>
            {
                PlatformSupportPluginsContainer.Items.Add(plugin);
                (PlatformSupportPluginsContainer.Visibility, PlatformSupportPluginsPlaceholder.Visibility) = PlatformSupportPluginsContainer.Items.Count == 0 ? (Visibility.Collapsed, Visibility.Visible) : (Visibility.Visible, Visibility.Collapsed);
            },
            plugin =>
            {
                PlatformSupportPluginsContainer.Items.Remove(plugin);
                (PlatformSupportPluginsContainer.Visibility, PlatformSupportPluginsPlaceholder.Visibility) = PlatformSupportPluginsContainer.Items.Count == 0 ? (Visibility.Collapsed, Visibility.Visible) : (Visibility.Visible, Visibility.Collapsed);
            });
        viewModel.PluginManager.Subscribe<MetadataPlugin>(
            plugin =>
            {
                MetadataPluginsContainer.Items.Add(plugin);
                MetadataPluginsSelectedComboBox.Items.Add(plugin.Name);

                (MetadataPluginsContainer.Visibility, MetadataPluginsPlaceholder.Visibility, MetadataPluginsSelected.Visibility) = MetadataPluginsContainer.Items.Count == 0 ? (Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed) : (Visibility.Visible, Visibility.Collapsed, Visibility.Visible);
            },
            plugin =>
            {
                MetadataPluginsContainer.Items.Remove(plugin);
                MetadataPluginsSelectedComboBox.Items.Remove(plugin.Name);

                (MetadataPluginsContainer.Visibility, MetadataPluginsPlaceholder.Visibility, MetadataPluginsSelected.Visibility) = MetadataPluginsContainer.Items.Count == 0 ? (Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed) : (Visibility.Visible, Visibility.Collapsed, Visibility.Visible);

                viewModel.Config.Downloads.SelectedMetadatePlugin = viewModel.PluginManager.GetLoadedOrDefault<MetadataPlugin>(viewModel.Config.Downloads.SelectedMetadatePlugin)?.Name;
            });
    }


    async void OnResetPluginConfigClick(object sender, RoutedEventArgs _)
    {
        IPlugin plugin = (IPlugin)((Button)sender).DataContext;
        await viewModel.ResetPluginConfigAsync(plugin);
    }
}