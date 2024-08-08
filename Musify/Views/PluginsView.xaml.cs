using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class PluginsView : Page
{
    readonly PluginsViewModel viewModel = App.Provider.GetRequiredService<PluginsViewModel>();

    public PluginsView()
    {
        InitializeComponent();
    }
}