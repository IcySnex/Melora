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
            PlatformSupportPluginsContainer.Items.Add,
            plugin => PlatformSupportPluginsContainer.Items.Remove(plugin));
    }


    async void OnResetPluginConfigClick(object sender, RoutedEventArgs _)
    {
        PlatformSupportPlugin plugin = (PlatformSupportPlugin)((Button)sender).DataContext;
        await viewModel.ResetPluginConfigAsync(plugin);
    }
}