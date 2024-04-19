using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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


    protected override void OnNavigatedTo(
        NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not string parameter)
            return;

        viewModel.Query = parameter;
        viewModel.SearchCommand.Execute(null);
    }
}