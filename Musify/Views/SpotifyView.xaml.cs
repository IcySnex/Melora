using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Musify.Models;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class SpotifyView : Page
{
    readonly SpotifyViewModel viewModel = App.Provider.GetRequiredService<SpotifyViewModel>();

    public SpotifyView()
    {
        InitializeComponent();
    }

    private void OnTracksSelectionChanged(object _, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
            viewModel.SelectedTracks.AddRange(e.AddedItems.Cast<Track>());

        if (e.RemovedItems.Count > 0)
            viewModel.SelectedTracks.RemoveRange(e.RemovedItems.Cast<Track>());
    }
}