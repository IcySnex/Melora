using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class PluginBundlesView : Page
{
    readonly PluginBundlesViewModel viewModel = App.Provider.GetRequiredService<PluginBundlesViewModel>();

    public PluginBundlesView()
    {
        InitializeComponent();
    }
}