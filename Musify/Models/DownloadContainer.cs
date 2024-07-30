using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Plugins.Models;

namespace Musify.Models;

public partial class DownloadContainer(
    DownloadableTrack track) : ObservableObject
{
    [ObservableProperty]
    int progress = 0;

    [ObservableProperty]
    bool isProcessing = false;

    public DownloadableTrack Track { get; } = track;
}