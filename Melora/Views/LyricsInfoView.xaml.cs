using GeniusAPI.Models;
using Melora.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Melora.Views;

public sealed partial class LyricsInfoView : Page
{
    readonly LyricsInfoViewModel viewModel = default!;

    public LyricsInfoView(
        LyricsInfoViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;

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
            GeniusArtist artist = viewModel.Track.FeaturedArtists[i];

            if (viewModel.Track.FeaturedArtists.Length != i)
                ArtistsBlock.Inlines.Add(CreateRun(", "));

            ArtistsBlock.Inlines.Add(CreateHyperlink(artist.Name, artist.Url));
        }
    }


    void OnCopyLyricsHyperlinkClicked(Hyperlink _, HyperlinkClickEventArgs _1) =>
        viewModel.CopyLyricsToClipboardCommand.Execute(null);
}