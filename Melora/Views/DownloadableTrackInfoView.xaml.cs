using Melora.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Views;

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