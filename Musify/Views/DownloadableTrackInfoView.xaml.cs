using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class DownloadableTrackInfoView : Page
{
    readonly DownloadableTrackInfoViewModel viewModel = default!;

    public DownloadableTrackInfoView(
        DownloadableTrackInfoViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
    }
}