using Microsoft.Extensions.Logging;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.PlatformSupport.Spotify.Internal;
using System.ComponentModel;

namespace Musify.PlatformSupport.Spotify;

public class SpotifyPlugin : PlatformSupportPlugin
{
    readonly SpotifyWrapper wrapper;

    public SpotifyPlugin(
        IPluginConfig? config,
        ILogger<PlatformSupportPlugin>? logger) : base(
            "Spotify",
            "M255.8 89.1c42.8 3.6 77.9 9.3 114.5 18.5 40 10.1 69.4 20.7 102.6 36.9 24 11.9 32.1 17.9 36.8 27.9 2.3 4.7 2.4 5.7 2.4 13.4 0 7.9-.1 8.5-2.7 13.2-6.8 12.6-21.1 18.9-34.6 15.6-2.8-.7-13.4-5.3-23.6-10.2-10.1-4.9-22.7-10.8-28-13-47.5-19.8-104-32.8-168.4-38.7-16.9-1.6-24-1.8-58.5-1.8-43.4 0-57.2.8-88.2 5.2-22.8 3.2-31 4.7-50.6 9.5-14.8 3.6-17.7 4.1-25.4 4.1-10.9.1-15.6-1.6-22.4-7.9-10.5-9.7-12.8-23.2-6-35.8 6.6-12.5 19.8-18.5 58.1-26.4 23.7-4.9 45.4-7.9 74.4-10.3 26.2-2.4 93.6-2.4 119.6-.2zm-37.4 124.5c58.9 4.1 104.6 13.3 153.9 31 30.7 11 63.7 27.1 77 37.5 8.5 6.7 13.2 17.4 11.5 26-2.8 14.2-16.7 22.5-31.3 18.7-2.7-.7-13.5-5.7-24.7-11.3-56.3-28.6-114.7-44.2-184.5-49.3-49.9-3.6-103.2 1-149.7 13.2-7.9 2-11 2.4-18.5 2.4-8.3.1-9.3-.1-13.5-2.1-5.3-2.6-8.8-6.3-11.5-12.2-1.6-3.5-1.8-5.1-1.6-10.2.3-7.5 2.6-12.3 7.9-17.1 6.8-6.1 14.9-9.2 39-14.8 28.3-6.5 49.5-9.3 91.9-12.5 10.3-.7 38.9-.3 54.1.7zm-27.6 119.2c36.6-.2 62.7 2.3 96.7 9.3 28.4 5.8 62.8 17.7 90.2 31.1 27.7 13.5 36.8 21.8 36.8 33.2 0 10.1-8.5 18.1-19 18.1-4.9 0-11-2.4-27.7-10.8-43-21.8-81.9-33.2-133.9-38.8-15.1-1.6-74.4-1.4-90.3.4-17.5 2-43.3 5.7-58.1 8.6-17.3 3.2-21.9 3.8-26.8 3-5.3-.9-8.8-2.5-12.1-5.7-7.3-7.1-7.3-17.8-.2-25.1 6.4-6.6 15.5-9.5 46.9-14.7 23.6-3.9 33.2-5.1 51.5-6.7l17.7-1.4c1.8-.1 14.5-.5 28.3-.5z",
            config,
            () => new PlatformSupportPluginConfig(
                items:
                [
                    new("Save Lyrics", "Whether to search & save lyrics from Genius automatically", true),
                    new("Save Artwork", "Whether to download & save the artwork", true),
                    new("Prefer Genius-Genre", "Whether to prefer fetching the track genre from Genius instead of what Spotify provides", false),
                    new("Playlist As Album", "Whether to set the playlist name as the album if possible", false),
                    new("Search Market", "The region code for the Spotify search market", "US"),
                    new("Client ID", "The client ID for the Spotify Web API", "75e1749b48dd4466858cf28ab32b1c8a"),
                    new("Client Secret", "The client secret for the Spotify Web API", "b884202c63af4bcbbcac91cfcf16e6c8"),
                    new("Genius Access Token", "The access token used to search and fetch track lyrics/genres on Genius", "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr")
                ],
                quality: Quality._160kbps,
                format: Format.mp3,
                searchResultsLimit: null,
                searchResultsSorting: Sorting.Default,
                searchResultsSortDescending: false),
            logger)
    {
        wrapper = new(Config, logger);

        Config.PropertyChanged += OnConfigPropertyChanged;
    }

    public SpotifyPlugin(
        IPluginConfig? config) : this(config, null)
    { }

    public SpotifyPlugin(
        ILogger<PlatformSupportPlugin>? logger) : this(null, logger)
    { }

    public SpotifyPlugin() : this(null, null)
    { }


    void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Client ID":
            case "Client Secret":
                wrapper.AuthenticateClient();
                break;
            case "Genius Access Token":
                wrapper.AuthenticateGeniusClient();
                break;
        }
    }


    public override async Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyPlugin-SearchAsync] Getting Spotify search type...");
        progress.Report("Preparing search...");

        SpotifySearchType type = SpotifyWrapper.GetSearchType(query, out string? id);
        switch (type)
        {
            case SpotifySearchType.Track:
                    SearchResult trackResult = await wrapper.SearchTrackAsync(id!, progress, cancellationToken);
                    return [trackResult];
            case SpotifySearchType.Album:
                    IEnumerable<SearchResult> albumResults = await wrapper.SearchAlbumAsync(id!, progress, cancellationToken);
                    return albumResults;
            case SpotifySearchType.Playlist:
                    IEnumerable<SearchResult> playlistResults = await wrapper.SearchPlaylistAsync(id!, progress, cancellationToken);
                    return playlistResults;
            case SpotifySearchType.Artist:
                    IEnumerable<SearchResult> artistResults = await wrapper.SearchArtistAsync(id!, progress, cancellationToken);
                    return artistResults;
            case SpotifySearchType.Query:
                    IEnumerable<SearchResult> querytResults = await wrapper.SearchQueryAsync(query, progress, cancellationToken);
                    return querytResults;
        }
        
        return [];
    }

    public override async Task<IEnumerable<DownloadableTrack>> PrepareDownloadsAsync(
        IEnumerable<SearchResult> searchResults,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyPlugin-PrepareDownloadsAsync] Preparing search results for download...");

        SearchResult[] indexableSearchResults = searchResults.ToArray();
        DownloadableTrack[] results = new DownloadableTrack[indexableSearchResults.Length];

        await Parallel.ForEachAsync(
            indexableSearchResults.Select((searchRresult, index) => new { searchRresult, index }),
            cancellationToken,
            async (item, token) =>
            {
                token.ThrowIfCancellationRequested();

                progress.Report($"Preparing downloads [{item.index}/{indexableSearchResults.Length}]...");

                DownloadableTrack track = await wrapper.PrepareDownloadAsync(item.searchRresult, this, token);
                results[item.index] = track;
            });
        return results;
    }

    public override Task<Stream> GetStreamAsync(
        DownloadableTrack track,
        CancellationToken cancellationToken = default) =>
        wrapper.GetStreamAsync(track.Id, cancellationToken);
}