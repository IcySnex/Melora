using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class PluginBundleInfoView : Page
{
    readonly PluginBundleInfoViewModel viewModel = default!;

    public PluginBundleInfoView(
        PluginBundleInfoViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
    }
}