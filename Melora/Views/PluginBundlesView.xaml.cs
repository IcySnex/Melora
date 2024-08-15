using Melora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Views;

public sealed partial class PluginBundlesView : Page
{
    readonly PluginBundlesViewModel viewModel = App.Provider.GetRequiredService<PluginBundlesViewModel>();

    public PluginBundlesView()
    {
        InitializeComponent();
    }
}