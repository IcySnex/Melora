using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class SpotifyView : Page
{
    readonly SpotifyViewModel viewModel = App.Provider.GetRequiredService<SpotifyViewModel>();

    public SpotifyView()
    {
        InitializeComponent();

        viewModel.SelectedTracks = TrackContainer.SelectedItems;
    }
}