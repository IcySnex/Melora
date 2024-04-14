using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class YouTubeView : Page
{
    readonly YouTubeViewModel viewModel = App.Provider.GetRequiredService<YouTubeViewModel>();

    public YouTubeView()
    {
        InitializeComponent();

        viewModel.SelectedVideos = VideoContainer.SelectedItems;
    }
}