using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class DownloadsView : Page
{
    readonly DownloadsViewModel viewModel = App.Provider.GetRequiredService<DownloadsViewModel>();

    public DownloadsView()
    {
        InitializeComponent();
    }
}