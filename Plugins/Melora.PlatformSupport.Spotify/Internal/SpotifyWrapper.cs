using GeniusAPI;
using GeniusAPI.Models;
using Melora.Plugins.Abstract;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Streaming;

namespace Melora.PlatformSupport.Spotify.Internal;

internal partial class SpotifyWrapper
{
    [GeneratedRegex(@"^(?:http(s)?:\/\/open\.spotify\.com\/(?:user\/[a-zA-Z0-9]+\/)?(?:track|album|playlist|artist)\/|spotify:(?:user:[a-zA-Z0-9]+:)?(?:track|album|playlist|artist):)([a-zA-Z0-9]+)")]
    private static partial Regex SpotifyUrlRegex();

    [GeneratedRegex(@"(?:music\.youtube\.com/watch.*?v=)([^&/?]+)(?:&|/|$)")]
    private static partial Regex YtmSongVideoIdRegex();

    private static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

    private static DateTime GetDateTime(
        string dateTimeString,
        string precision) =>
        precision switch
        {
            "day" => DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", null),
            "month" => DateTime.ParseExact(dateTimeString, "yyyy-MM", null),
            "year" => DateTime.ParseExact(dateTimeString, "yyyy", null),
            _ => DateTime.MinValue
        };


    public static bool IsValidYtmSongVideoId(
        string songVideoId) =>
        songVideoId.Length == 11 && songVideoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

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
        if (query.Contains("artist"))
            return SpotifySearchType.Artist;

        throw new Exception("URL returned good ID but is not a track/album/playlist."); // ???
    }


    public static string? GetLowResArtworkUrl(
        IEnumerable<Image> artworks) =>
        artworks.MinBy(artwork => artwork.Width * artwork.Height)?.Url;


    public static string? GetHighResArtworklUrl(
        IEnumerable<Image> artworks) =>
        artworks.MaxBy(artwork => artwork.Width * artwork.Height)?.Url;


    readonly PlatformSupportPluginConfig config;
    readonly ILogger<IPlugin>? logger;

    SpotifyClient client = default!;
    GeniusClient geniusClient = default!;
    YouTubeMusicClient ytmClient = default!;

    public SpotifyWrapper(
        PlatformSupportPluginConfig config,
        ILogger<IPlugin>? logger = null)
    {
        this.config = config;
        this.logger = logger;

        AuthenticateClient();
        AuthenticateGeniusClient();
        AuthenticatsYtmClient();

        logger?.LogInformation("[SpotifyWrapper-.ctor] SpotifyWrapper has been initialized");
    }


    public void AuthenticateClient()
    {
        string clientId = config.GetStringOption("Client ID");
        string clientSecret = config.GetStringOption("Client Secret");

        client = new(SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret)));

        logger?.LogInformation("[SpotifyWrapper-AuthenticateClient] Client has been authenticated.");
    }

    public void AuthenticateGeniusClient()
    {
        string geniusAccessToken = config.GetStringOption("Genius Access Token");

        if (geniusClient is null)
            geniusClient = logger is null ? new(geniusAccessToken) : new(geniusAccessToken, logger);
        else
            geniusClient.AccessToken = geniusAccessToken;

        logger?.LogInformation("[SpotifyWrapper-AuthenticateGeniusClient] Genius client has been authenticated.");
    }

    public void AuthenticatsYtmClient()
    {
        string geographicalLocation = config.GetStringOption("Search Market");
        string visitorData = config.GetStringOption("YTM Visitor Data");
        string poToken = config.GetStringOption("YTM Po Token");

        if (ytmClient is null)
            ytmClient = new(
                geographicalLocation,
                string.IsNullOrWhiteSpace(visitorData) ? null : visitorData,
                string.IsNullOrWhiteSpace(poToken) ? null : poToken);
        else
        {
            ytmClient.GeographicalLocation = geographicalLocation;
            ytmClient.VisitorData = string.IsNullOrWhiteSpace(visitorData) ? null : visitorData;
            ytmClient.PoToken = string.IsNullOrWhiteSpace(poToken) ? null : poToken;
        }

        logger?.LogInformation("[SpotifyWrapper-AuthenticatsYtmClient] YouTube Music client has been authenticated.");
    }


    public async Task<SearchResult> SearchTrackAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-SearchTrackAsync] Searching for track...");
        progress.Report("Searching for track...");

        FullTrack track = await client.Tracks.Get(id, cancellationToken);

        SearchResult result = new(
            title: track.Name,
            artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
            duration: TimeSpan.FromMilliseconds(track.DurationMs),
            imageUrl: GetLowResArtworkUrl(track.Album.Images),
            id: track.Id,
            items: new()
            {
                { "PrimaryArtistId", track.Artists[0].Id },
                { "Explicit", track.Explicit },
                { "ReleaseDate", track.Album.ReleaseDate },
                { "ReleaseDatePrecision", track.Album.ReleaseDatePrecision },
                { "AlbumName", track.Album.Name },
                { "AlbumTotalTracks", track.Album.TotalTracks },
                { "TrackNumber", track.TrackNumber },
                { "FullArtwork", GetHighResArtworklUrl(track.Album.Images) }
            });
        return result;
    }

    public async Task<IEnumerable<SearchResult>> SearchAlbumAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-SearchAlbumAsync] Searching for album...");
        progress.Report("Searching for album...");

        FullAlbum album = await client.Albums.Get(id, cancellationToken);
        IAsyncEnumerable<SimpleTrack> tracks = client.Paginate(album.Tracks, null, cancellationToken);

        int totalTracksToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), album.Tracks.Total.GetValueOrDefault(0));
        string leftTracksToBuffer = totalTracksToBuffer != 0 ? $"/{totalTracksToBuffer}" : string.Empty;

        string? albumArtwork = GetLowResArtworkUrl(album.Images);
        string? albumFullArtwork = GetHighResArtworklUrl(album.Images);

        return await SearchResult.BufferAsync(
            tracks,
            totalTracksToBuffer,
            (SimpleTrack track, int index) =>
            {
                progress.Report($"Buffering tracks {index}{leftTracksToBuffer}...");

                return new SearchResult(
                    title: track.Name,
                    artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
                    duration: TimeSpan.FromMilliseconds(track.DurationMs),
                    imageUrl: albumArtwork,
                    id: track.Id,
                    items: new()
                    {
                        { "PrimaryArtistId", track.Artists[0].Id },
                        { "Explicit", track.Explicit },
                        { "ReleaseDate", album.ReleaseDate },
                        { "ReleaseDatePrecision", album.ReleaseDatePrecision },
                        { "AlbumName", album.Name },
                        { "AlbumTotalTracks", album.TotalTracks },
                        { "TrackNumber", track.TrackNumber },
                        { "FullArtwork", albumFullArtwork }
                    });
            },
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchPlaylistAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-SearchPlaylistAsync] Searching for playlist...");
        progress.Report("Searching for playlist...");

        FullPlaylist playlist = await client.Playlists.Get(id, cancellationToken);
        Paging<PlaylistTrack<IPlayableItem>> playlistTracks = await client.Playlists.GetItems(id, cancellationToken);
        IAsyncEnumerable<PlaylistTrack<IPlayableItem>> tracks = client.Paginate(playlistTracks, null, cancellationToken);

        int totalTracksToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), playlistTracks.Total.GetValueOrDefault(0));
        string leftTracksToBuffer = totalTracksToBuffer != 0 ? $"/{totalTracksToBuffer}" : string.Empty;

        return await SearchResult.BufferAsync(
            tracks,
            totalTracksToBuffer,
            (PlaylistTrack<IPlayableItem> item, int index) =>
            {
                progress.Report($"Buffering tracks {index}{leftTracksToBuffer}...");

                if (item.Track is not FullTrack track || string.IsNullOrEmpty(track.Name))
                    return null;

                return new SearchResult(
                    title: track.Name,
                    artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
                    duration: TimeSpan.FromMilliseconds(track.DurationMs),
                    imageUrl: GetLowResArtworkUrl(track.Album.Images),
                    id: track.Id,
                    items: new()
                    {
                        { "PrimaryArtistId", track.Artists[0].Id },
                        { "Explicit", track.Explicit },
                        { "ReleaseDate", track.Album.ReleaseDate },
                        { "ReleaseDatePrecision", track.Album.ReleaseDatePrecision },
                        { "AlbumName", config.GetBoolOption("Playlist As Album") ? playlist.Name : track.Album.Name },
                        { "AlbumTotalTracks", track.Album.TotalTracks },
                        { "TrackNumber", track.TrackNumber },
                        { "FullArtwork", GetHighResArtworklUrl(track.Album.Images) }
                    });
            },
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchArtistAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-SearchPlaylistAsync] Searching for artist...");
        progress.Report("Searching for artist...");

        FullArtist artist = await client.Artists.Get(id, cancellationToken);
        ArtistsTopTracksResponse response = await client.Artists.GetTopTracks(id, new(config.GetStringOption("Search Market")), cancellationToken);

        string albumName = $"{artist.Name}'s Top Tracks";

        IEnumerable<SearchResult> results = response.Tracks.Select(track => new SearchResult(
            title: track.Name,
            artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
            duration: TimeSpan.FromMilliseconds(track.DurationMs),
            imageUrl: GetLowResArtworkUrl(track.Album.Images),
            id: track.Id,
            items: new()
            {
                { "PrimaryArtistId", track.Artists[0].Id },
                { "Explicit", track.Explicit },
                { "ReleaseDate", track.Album.ReleaseDate },
                { "ReleaseDatePrecision", track.Album.ReleaseDatePrecision },
                { "AlbumName", config.GetBoolOption("Playlist As Album") ? albumName : track.Album.Name },
                { "AlbumTotalTracks", track.Album.TotalTracks },
                { "TrackNumber", track.TrackNumber },
                { "FullArtwork", GetHighResArtworklUrl(track.Album.Images) }
            }));
        return config.SearchResultsLimit.HasValue ? results.Take(config.SearchResultsLimit.Value) : results;
    }

    public async Task<IEnumerable<SearchResult>> SearchQueryAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-SearchTrackAsync] Searching for query...");
        progress.Report("Searching for query...");

        SearchResponse response = await client.Search.Item(new(SearchRequest.Types.Track, query)
        {
            Market = config.GetStringOption("Search Market"),
            Limit = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), 50),
        }, cancellationToken);

        if (response.Tracks.Items is null)
            return [];

        IEnumerable<SearchResult> results = response.Tracks.Items.Select(track => new SearchResult(
            title: track.Name,
            artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
            duration: TimeSpan.FromMilliseconds(track.DurationMs),
            imageUrl: GetLowResArtworkUrl(track.Album.Images),
            id: track.Id,
            items: new()
            {
                { "PrimaryArtistId", track.Artists[0].Id },
                { "Explicit", track.Explicit },
                { "ReleaseDate", track.Album.ReleaseDate },
                { "ReleaseDatePrecision", track.Album.ReleaseDatePrecision },
                { "AlbumName", track.Album.Name },
                { "AlbumTotalTracks", track.Album.TotalTracks },
                { "TrackNumber", track.TrackNumber },
                { "FullArtwork", GetHighResArtworklUrl(track.Album.Images) }
            }));
        return results;
    }


    public async Task<DownloadableTrack> PrepareDownloadAsync(
        SearchResult searchResult,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-PrepareDownloadAsync] Preparing track '{id}' for download...", searchResult.Id);

        bool saveLyrics = config.GetBoolOption("Save Lyrics");

        string? fullArtwork = searchResult.GetItem<string?>("FullArtwork");
        bool @explicit = searchResult.GetItem<bool>("Explicit");
        DateTime releaseDate = GetDateTime(searchResult.GetItem<string>("ReleaseDate"), searchResult.GetItem<string>("ReleaseDatePrecision"));
        string albumName = searchResult.GetItem<string>("AlbumName");
        string? genre = await GetArtistGenreAsync(searchResult.GetItem<string>("PrimaryArtistId"), cancellationToken);
        string? lyrics = saveLyrics ? await GetLyricsAsync(searchResult.Title, searchResult.Artists.Split(',')[0], cancellationToken) : null;
        int trackNumber = searchResult.GetItem<int>("TrackNumber");
        int albumTotalTracks = searchResult.GetItem<int>("AlbumTotalTracks");

        return new DownloadableTrack(
            title: searchResult.Title,
            artists: searchResult.Artists,
            duration: searchResult.Duration,
            artworkUrl: fullArtwork,
            isExplicit: @explicit,
            releasedAt: releaseDate,
            album: albumName,
            genre: genre,
            lyrics: lyrics,
            trackNumber: trackNumber,
            totalTracks: albumTotalTracks,
            copyright: $"{searchResult.Title} - {searchResult.Artists} ({albumName}) ℗ {releaseDate.Year}, Provided to Spotify, Auto-generated by Melora.PlatformSupport.Spotify.SpotifyPlugin",
            comment: "Downloaded with Melora, Melora.PlatformSupport.Spotify.SpotifyPlugin",
            url: $"https://open.spotify.com/track/{searchResult.Id}",
            id: config.GetStringOption("YTM Search Format")
                .Replace("{title}", searchResult.Title)
                .Replace("{album}", albumName)
                .Replace("{artists}", searchResult.Artists)
                .Replace("{year}", releaseDate.Year.ToString()));
    }

    readonly Dictionary<string, string?> artistGenreCache = [];

    async Task<string?> GetArtistGenreAsync(
        string artistId,
        CancellationToken cancellationToken = default)
    {
        if (artistGenreCache.TryGetValue(artistId, out string? genre))
            return genre;

        logger?.LogInformation("[SpotifyWrapper-GetArtistGenreAsync] Genre was not found in cache. Getting artist...");

        FullArtist artist = await client.Artists.Get(artistId, cancellationToken);
        genre = artist.Genres.FirstOrDefault() is string artistGenre ? TextInfo.ToTitleCase(artistGenre) : null;

        artistGenreCache[artistId] = genre;
        return genre;
    }

    async Task<string?> GetLyricsAsync(
        string title,
        string artist,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-GetGeniusTrackInfoAsync] Getting track info on Genius...");

        GeniusTrackInfo? trackInfo = await geniusClient.GetTrackInfoAsync(title, artist, cancellationToken);
        if (trackInfo is null)
            return null;

        return trackInfo.Lyrics;
    }


    public async Task<Stream> GetStreamAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyWrapper-GetStreamAsync] Checking YouTube Music SongVideo ID...");
        Match videoIdMatch = YtmSongVideoIdRegex().Match(id);
        string? ytmId = videoIdMatch.Success ? WebUtility.UrlDecode(videoIdMatch.Groups[1].Value) : null;

        if (string.IsNullOrWhiteSpace(ytmId) || !IsValidYtmSongVideoId(ytmId))
        {
            logger?.LogInformation("[SpotifyWrapper-GetStreamAsync] Searching track on YouTube Music...");
            SongSearchResult searchResult = (await ytmClient.SearchAsync<SongSearchResult>(id, 1, cancellationToken)).FirstOrDefault()
                ?? throw new Exception("Could not find track on YouTube Music.");

            ytmId = searchResult.Id;
        }

        logger?.LogInformation("[SpotifyWrapper-GetStreamAsync] Getting songVideo stream...");
        StreamingData streamingData = await ytmClient.GetStreamingDataAsync(ytmId, cancellationToken);

        if (streamingData.IsLiveContent)
            throw new Exception("Live content is not supported.");

        AudioStreamInfo? stream = streamingData.StreamInfo
            .OfType<AudioStreamInfo>()
            .MinBy(streamInfo => Math.Abs((int)config.Quality - streamInfo.Bitrate / 1000))
            ?? throw new("Could not find any suitable audio streams in streaming data.");

        return await stream.GetStreamAsync(cancellationToken);
    }
}