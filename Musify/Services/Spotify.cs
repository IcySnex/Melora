using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using SpotifyAPI.Web;
using System.Text.RegularExpressions;
using Windows.Media.Playlists;

namespace Musify.Services;

public partial class Spotify
{
    [GeneratedRegex(@"^(?:http(s)?:\/\/open\.spotify\.com\/(?:user\/[a-zA-Z0-9]+\/)?(?:track|album|playlist)\/|spotify:(?:user:[a-zA-Z0-9]+:)?(?:track|album|playlist):)([a-zA-Z0-9]+)")]
    private static partial Regex SpotifyUrlRegex();

    public static string? GetId(
        string url)
    {
        Match match = SpotifyUrlRegex().Match(url);
        return match.Success ? match.Groups[2].Value : null;
    }

    public static SpotifySearchType GetSearchType(
        string query,
        out string? id)
    {
        id = null;
        if (GetId(query) is not string urlId)
            return SpotifySearchType.Query;

        id = urlId;
        query = query.ToLowerInvariant();
        if (query.Contains("track"))
            return SpotifySearchType.Track;
        if (query.Contains("album"))
            return SpotifySearchType.Album;
        if (query.Contains("playlist"))
            return SpotifySearchType.Playlist;

        throw new Exception("URL returned good ID but is not a track/album/playlist."); // ???
    }


    readonly ILogger<Spotify> logger;
    readonly Config config;

    readonly SpotifyClient client;

    public Spotify(
        ILogger<Spotify> logger,
        IOptions<Config> config)
    {
        this.logger = logger;
        this.config = config.Value;

        client = new(CreateConfig(config.Value.Advanced.SpotifyClientId, config.Value.Advanced.SpotifyClientSecret));

        logger.LogInformation("[Spotify-.ctor] Spotify has been initialized");
    }


    SpotifyClientConfig CreateConfig(
        string clientId,
        string secret)
    {
        logger.LogInformation("[Spotify-CreateConfig] Creating config with authentication...");

        return SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, secret));
    }


    public Task<FullTrack> SearchTrackAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[Spotify-SearchTrackAsync] Searching for track...");
        progress?.Report("Searching for track...");

        return client.Tracks.Get(id!, cancellationToken);
    }

    public async Task<IAsyncEnumerable<FullTrack>> SearchAlbumAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[Spotify-SearchAlbumAsync] Searching for album...");
        progress?.Report("Searching for album...");

        FullAlbum album = await client.Albums.Get(id!, cancellationToken);

        IAsyncEnumerable<SimpleTrack> tracks = client.Paginate(album.Tracks, null, cancellationToken);
        return tracks
            .Select(track => new FullTrack()
        {
            Album = new SimpleAlbum()
            {
                AlbumType = album.AlbumType,
                Artists = album.Artists,
                AvailableMarkets = album.AvailableMarkets,
                ExternalUrls = album.ExternalUrls,
                Href = album.Href,
                Id = album.Id,
                Images = album.Images,
                Name = album.Name,
                ReleaseDate = album.ReleaseDate,
                ReleaseDatePrecision = album.ReleaseDatePrecision,
                Restrictions = album.Restrictions,
                TotalTracks = album.TotalTracks,
                Type = album.Type,
            },
            Artists = track.Artists,
            AvailableMarkets = track.AvailableMarkets,
            DiscNumber = track.DiscNumber,
            DurationMs = track.DurationMs,
            Explicit = track.Explicit,
            ExternalUrls = track.ExternalUrls,
            Href = track.Href,
            Id = track.Id,
            IsPlayable = track.IsPlayable,
            LinkedFrom = track.LinkedFrom,
            Name = track.Name,
            PreviewUrl = track.PreviewUrl,
            TrackNumber = track.TrackNumber,
            Type = track.Type,
            Uri = track.Uri,
        });
    }

    public async Task<IAsyncEnumerable<FullTrack>> SearchPlaylistAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[Spotify-SearchPlaylistAsync] Searching for playlist...");
        progress?.Report("Searching for playlist...");

        FullPlaylist playlist = await client.Playlists.Get(id!, cancellationToken);
        if (playlist.Tracks is null)
        {
            logger.LogError("[Spotify-SearchPlaylistAsync] Playlist tracks is null");
            throw new NullReferenceException("Playlist tracks is null.");
        }

        return client.Paginate(playlist.Tracks, null, cancellationToken)
            .Where(track => track.Track is not null && track.Track.Type == ItemType.Track)
            .Select(track => (FullTrack)track.Track);
    }

    public async Task<List<FullTrack>> SearchQueryAsync(
        string query,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[Spotify-SearchAsync] Searching for query...");
        progress?.Report("Searching for query...");

        SearchRequest request = new(SearchRequest.Types.Track, query)
        {
            Market = config.Advanced.SpotifySearchMarket,
            Limit = config.Advanced.SpotifyQuerySearchResultsLimit
        };
        SearchResponse response = await client.Search.Item(request, cancellationToken);

        return response.Tracks.Items ?? [];
    }
}