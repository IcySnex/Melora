﻿using GeniusAPI;
using GeniusAPI.Models;
using Melora.Plugins.Abstract;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Melora.PlatformSupport.Spotify.Internal;

internal partial class SpotifyWrapper
{
    [GeneratedRegex(@"^(?:http(s)?:\/\/open\.spotify\.com\/(?:user\/[a-zA-Z0-9]+\/)?(?:track|album|playlist|artist)\/|spotify:(?:user:[a-zA-Z0-9]+:)?(?:track|album|playlist|artist):)([a-zA-Z0-9]+)")]
    private static partial Regex SpotifyUrlRegex();

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
    HttpClient httpClient = default!;

    public SpotifyWrapper(
        PlatformSupportPluginConfig config,
        ILogger<IPlugin>? logger = null)
    {
        this.config = config;
        this.logger = logger;

        AuthenticateClient();
        AuthenticateGeniusClient();
        AuthenticatsHttpClient();

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

    public void AuthenticatsHttpClient()
    {
        httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Origin", "https://spotifydown.com");
        httpClient.DefaultRequestHeaders.Add("Referer", "https://spotifydown.com/");

        logger?.LogInformation("[SpotifyWrapper-AuthenticatsHttpClient] HTTP client has been authenticated.");
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
            id: searchResult.Id);
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
        logger?.LogInformation("[SpotifyWrapper-GetStreamAsync] Getting track stream...");
        string url = $"https://api.spotifydown.com/download/{id}";

        string response = await httpClient.GetStringAsync(url, cancellationToken);
        SpotifyDownDownload downloadInfo = JsonConvert.DeserializeObject<SpotifyDownDownload>(response) ?? throw new Exception("Deserialized SpotifyDown response was null.");

        if (!downloadInfo.IsSuccessful)
            throw new Exception($"SpotifyDown returned unsuccessful status. Error message: {downloadInfo.ErrorMessage}.");
        if (!downloadInfo.MetaData.IsSuccessful)
            throw new Exception($"SpotifyDown returned unsuccessful meta data status.");

        return await httpClient.GetStreamAsync(downloadInfo.StreamUrl, cancellationToken);
    }
}