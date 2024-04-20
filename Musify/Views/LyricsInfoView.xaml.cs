using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using Musify.Models;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class LyricsInfoView : Page
{
    LyricsInfoViewModel viewModel = default!;

    public LyricsInfoView()
    {
        InitializeComponent();
    }


    protected override void OnNavigatedTo(
        NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not LyricsInfoViewModel parameter)
            return;

        viewModel = parameter;
        SetArtistsInlines();
    }


    static Run CreateRun(
        string text) =>
        new() { Text = text };

    static Hyperlink CreateHyperlink(
        string text,
        string url) =>
        new() { Inlines = { CreateRun(text) }, NavigateUri = new(url) };

    void SetArtistsInlines()
    {
        ArtistsBlock.Inlines.Add(CreateHyperlink(viewModel.Track.PrimaryArtist.Name, viewModel.Track.PrimaryArtist.Url));
        for (int i = 0; i < viewModel.Track.FeaturedArtists.Length; i++)
        {
            LyricsArtist artist = viewModel.Track.FeaturedArtists[i];

            if (viewModel.Track.FeaturedArtists.Length != i)
                ArtistsBlock.Inlines.Add(CreateRun(", "));

            ArtistsBlock.Inlines.Add(CreateHyperlink(artist.Name, artist.Url));
        }
    }
}