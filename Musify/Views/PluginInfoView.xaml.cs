using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class PluginInfoView : Page
{
    readonly PluginInfoViewModel viewModel = default!;

    public PluginInfoView(
        PluginInfoViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
    }
}