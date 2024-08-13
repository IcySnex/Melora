using Microsoft.UI.Xaml.Controls;
using Melora.ViewModels;

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