using Melora.Plugins.Models;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Views;

public sealed partial class DownloadableChangeIdView : Page
{
    readonly DownloadableTrack track = default!;

    public DownloadableChangeIdView(
        DownloadableTrack track)
    {
        InitializeComponent();

        this.track = track;
    }
}