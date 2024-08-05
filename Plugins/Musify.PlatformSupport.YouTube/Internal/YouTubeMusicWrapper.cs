using GeniusAPI;
using GeniusAPI.Models;
using Microsoft.Extensions.Logging;
using Musify.Plugins.Abstract;
using Musify.Plugins.Models;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;

namespace Musify.PlatformSupport.YouTube.Internal;

internal partial class YouTubeMusicWrapper
{
    [GeneratedRegex(@"(?:music\.youtube\.com/watch.*?v=)([^&/?]+)(?:&|/|$)")]
    private static partial Regex YouTubeMusicSongVideoIdRegex();

    [GeneratedRegex(@"(?:music\.youtube\.com/playlist.*?list=|music\.youtube\.com/watch.*?list=)([^&/?]+)(?:&|/|$)")]
    private static partial Regex YouTubeMusicPlaylistIdRegex();

    [GeneratedRegex(@"music\.youtube\.com/channel/(.*?)(?:\?|&|/|$)")]
    private static partial Regex YouTubeMusicArtistIdRegex();


    public static bool IsValidSongVideoId(
        string songVideoId) =>
        songVideoId.Length == 11 && songVideoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidPlaylistId(
        string playlistId) =>
        playlistId.Length >= 2 && playlistId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidArtistId(
        string channelId) =>
        channelId.StartsWith("UC", StringComparison.Ordinal) && channelId.Length == 24 && channelId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');


    public static YouTubeMusicSearchType GetSearchType(
        string query,
        out string? id)
    {
        Match videoIdMatch = YouTubeMusicSongVideoIdRegex().Match(query);
        id = videoIdMatch.Success ? WebUtility.UrlDecode(videoIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidSongVideoId(id))
            return YouTubeMusicSearchType.SongVideo;

        Match playlistIdMatch = YouTubeMusicPlaylistIdRegex().Match(query);
        id = playlistIdMatch.Success ? WebUtility.UrlDecode(playlistIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidPlaylistId(id))
            return id.StartsWith("OLAK5uy_") ? YouTubeMusicSearchType.Album : YouTubeMusicSearchType.CommunityPlaylist;

        Match channelIdMatch = YouTubeMusicArtistIdRegex().Match(query);
        id = channelIdMatch.Success ? WebUtility.UrlDecode(channelIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidArtistId(id))
            return YouTubeMusicSearchType.Artist;

        id = null;
        return YouTubeMusicSearchType.Query;
    }


    public static string? GetLowResThumbnailUrl(
        IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.MinBy(thumbnail => thumbnail.Width * thumbnail.Height)?.Url;


    public static string? GetHighResThumbnailUrl(
        IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.MaxBy(thumbnail => thumbnail.Width * thumbnail.Height)?.Url;


    readonly int pluginHash;
    readonly PlatformSupportPluginConfig config;
    readonly ILogger<IPlugin>? logger;

    YouTubeMusicClient client = default!;
    YoutubeClient youTubeClient = default!;
    GeniusClient geniusClient = default!;

    public YouTubeMusicWrapper(
        int pluginHash,
        PlatformSupportPluginConfig config,
        ILogger<IPlugin>? logger = null)
    {
        this.pluginHash = pluginHash;
        this.config = config;
        this.logger = logger;

        AuthenticateClient();
        AuthenticateGeniusClient();
        AuthenticateYouTubeClient();

        logger?.LogInformation("[YouTubeMusicWrapper-.ctor] YouTubeMusicWrapper has been initialized");
    }


    public void AuthenticateClient()
    {
        string geographicalLocation = config.GetItem<string>("Geographical Location");

        client = logger is null ? new(geographicalLocation) : new(logger, geographicalLocation);

        logger?.LogInformation("[YouTubeMusicWrapper-AuthenticateClient] Client has been authenticated.");
    }

    public void AuthenticateGeniusClient()
    {
        string geniusAccessToken = config.GetItem<string>("Genius Access Token");

        if (geniusClient is null)
            geniusClient = logger is null ? new(geniusAccessToken) : new(geniusAccessToken, logger);
        else
            geniusClient.AccessToken = geniusAccessToken;

        logger?.LogInformation("[SpotifyWrapper-AuthenticateGeniusClient] Genius client has been authenticated.");
    }

    public void AuthenticateYouTubeClient()
    {
        youTubeClient = new();

        logger?.LogInformation("[SpotifyWrapper-AuthenticateYouTubeClient] YouTube client has been authenticated.");
    }


    public async Task<SearchResult> SearchSongVideoAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-SearchSongVideoAsync] Searching for song/video...");
        progress.Report("Searching for song/video...");

        SongVideoInfo songVideo = await client.GetSongVideoInfoAsync(id, cancellationToken);

        SearchResult result = new(
            title: songVideo.Name,
            artists: string.Join(", ", songVideo.Artists.Select(artist => artist.Name)),
            duration: songVideo.Duration,
            imageUrl: GetLowResThumbnailUrl(songVideo.Thumbnails),
            id: songVideo.Id,
            items: new()
            {
                { "Explicit", !songVideo.IsFamiliyFriendly },
                { "AlbumName", null },
                { "TrackNumber", 0 },
                { "TotalTracks", 0 }
            });
        return result;
    }

    public async Task<IEnumerable<SearchResult>> SearchAlbumAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-SearchAlbumAsync] Searching for album...");
        progress.Report("Searching for album...");

        string browseId = await client.GetAlbumBrowseIdAsync(id, cancellationToken);
        AlbumInfo album = await client.GetAlbumInfoAsync(browseId, cancellationToken);

        string albumArtists = string.Join(", ", album.Artists.Select(artist => artist.Name));
        string? albumThumbnail = GetLowResThumbnailUrl(album.Thumbnails);

        IEnumerable<SearchResult> results = album.Songs
            .Where(song => song.Id is not null)
            .Select(song => new SearchResult(
                title: song.Name,
                artists: albumArtists,
                duration: song.Duration,
                imageUrl: albumThumbnail,
                id: song.Id!,
                items: new()
                {
                    { "Explicit", song.IsExplicit },
                    { "AlbumName", album.Name },
                    { "TrackNumber", song.SongNumber ?? Array.IndexOf(album.Songs, song) },
                    { "TotalTracks", album.SongCount }
                }));
        return config.SearchResultsLimit.HasValue ? results.Take(config.SearchResultsLimit.Value) : results;
    }

    public async Task<IEnumerable<SearchResult>> SearchCommunityPlaylistAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-SearchCommunityPlaylistAsync] Searching for community playlist...");
        progress.Report("Searching for community playlist...");

        string browseId = client.GetCommunityPlaylistBrowseId(id);
        CommunityPlaylistInfo playlist = await client.GetCommunityPlaylistInfoAsync(browseId, cancellationToken);

        bool playlistAsAlbum = config.GetItem<bool>("Playlist As Album");

        IEnumerable<SearchResult> results = playlist.Songs
            .Where(song => song.Id is not null)
            .Select(song => new SearchResult(
                title: song.Name,
                artists: string.Join(", ", song.Artists.Select(artist => artist.Name)),
                duration: song.Duration,
                imageUrl: GetLowResThumbnailUrl(song.Thumbnails),
                id: song.Id!,
                items: new()
                {
                    { "Explicit", song.IsExplicit },
                    { "AlbumName", playlistAsAlbum ? playlist.Name : song.Album?.Name },
                    { "TrackNumber", playlistAsAlbum ? Array.IndexOf(playlist.Songs, song) : 0 },
                    { "TotalTracks", playlistAsAlbum ? playlist.SongCount : 0 }
                }));
        return config.SearchResultsLimit.HasValue ? results.Take(config.SearchResultsLimit.Value) : results;
    }

    public async Task<IEnumerable<SearchResult>> SearchArtistAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-SearchArtistAsync] Searching for artist...");
        progress.Report("Searching for artist...");

        ArtistInfo artist = await client.GetArtistInfoAsync(id, cancellationToken);
        if (artist.AllSongsPlaylistId is null)
            return [];

        string browseId = client.GetCommunityPlaylistBrowseId(artist.AllSongsPlaylistId);
        CommunityPlaylistInfo playlist = await client.GetCommunityPlaylistInfoAsync(browseId, cancellationToken);

        bool playlistAsAlbum = config.GetItem<bool>("Playlist As Album");

        IEnumerable<SearchResult> results = playlist.Songs
            .Where(song => song.Id is not null)
            .Select(song => new SearchResult(
                title: song.Name,
                artists: string.Join(", ", song.Artists.Select(artist => artist.Name)),
                duration: song.Duration,
                imageUrl: GetLowResThumbnailUrl(song.Thumbnails),
                id: song.Id!,
                items: new()
                {
                    { "Explicit", song.IsExplicit },
                    { "AlbumName", playlistAsAlbum ? $"{artist.Name}'s Songs" : song.Album?.Name },
                    { "TrackNumber", playlistAsAlbum ? Array.IndexOf(playlist.Songs, song) : 0 },
                    { "TotalTracks", playlistAsAlbum ? playlist.SongCount : 0 }
                }));
        return config.SearchResultsLimit.HasValue ? results.Take(config.SearchResultsLimit.Value) : results;
    }

    public async Task<IEnumerable<SearchResult>> SearchQueryAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-SearchQueryAsync] Searching for query...");
        progress.Report("Searching for query...");

        IEnumerable<Song> songs = await client.SearchAsync<Song>(query, cancellationToken);

        IEnumerable<SearchResult> results = songs.Select(song => new SearchResult(
            title: song.Name,
            artists: string.Join(", ", song.Artists.Select(artist => artist.Name)),
            duration: song.Duration,
            imageUrl: GetLowResThumbnailUrl(song.Thumbnails),
            id: song.Id,
            items: new()
            {
                { "Explicit", song.IsExplicit },
                { "AlbumName", song.Album?.Name },
                { "TrackNumber", 0 },
                { "TotalTracks", 0 }
            }));
        return config.SearchResultsLimit.HasValue ? results.Take(config.SearchResultsLimit.Value) : results;
    }


    public async Task<DownloadableTrack> PrepareDownloadAsync(
        SearchResult searchResult,
        CancellationToken cancellationToken = default)
    {
        bool saveLyrics = config.GetItem<bool>("Save Lyrics");
        bool fetchGenre = config.GetItem<bool>("Fetch Genre");

        (string? lyrics, string? genre) = (null, null);
        if (saveLyrics || fetchGenre)
            (lyrics, genre) = await GetGeniusTrackInfoAsync(searchResult.Title, searchResult.Artists.Split(',')[0], cancellationToken);

        SongVideoInfo songVideo = await client.GetSongVideoInfoAsync(searchResult.Id, cancellationToken);

        return new DownloadableTrack(
            title: searchResult.Title,
            artists: searchResult.Artists,
            duration: searchResult.Duration,
            artworkUrl: GetHighResThumbnailUrl(songVideo.Thumbnails),
            isExplicit: searchResult.GetItem<bool>("Explicit"),
            releasedAt: songVideo.UploadedAt,
            album: searchResult.GetItem<string?>("AlbumName"),
            genre: genre,
            lyrics: lyrics,
            url: $"https://music.youtube.com/watch?v={searchResult.Id}",
            trackNumber: searchResult.GetItem<int>("TrackNumber"),
            totalTracks: searchResult.GetItem<int>("TotalTracks"),
            id: searchResult.Id,
            pluginHash: pluginHash);
    }

    async Task<(string? lyrics, string? genre)> GetGeniusTrackInfoAsync(
        string title,
        string artist,
        CancellationToken cancellationToken = default)
    {
        bool saveLyrics = config.GetItem<bool>("Save Lyrics");
        bool fetchGenre = config.GetItem<bool>("Fetch Genre");

        logger?.LogInformation("[SpotifyWrapper-GetGeniusTrackInfoAsync] Getting track info on Genius...");

        GeniusTrackInfo? trackInfo = await geniusClient.GetTrackInfoAsync(title, artist, cancellationToken);
        if (trackInfo is null)
            return (null, null);

        return (saveLyrics ? trackInfo.Lyrics : null, fetchGenre ? trackInfo.Genres?.FirstOrDefault() : null);
    }


    public async Task<Stream> GetStreamAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeMusicWrapper-GetStreamAsync] Getting songVideo stream...");
        StreamManifest manifest = await youTubeClient.Videos.Streams.GetManifestAsync(id, cancellationToken);

        AudioOnlyStreamInfo? stream = manifest
            .GetAudioOnlyStreams()
            .MinBy(streamInfo => Math.Abs((int)config.Quality - streamInfo.Bitrate.KiloBitsPerSecond));
        if (stream is null)
        {
            logger?.LogError("[YouTubeMusicWrapper-GetStreamAsync] Could not find any suitable audio only streams in the streams manifest");
            throw new Exception("Could not find any suitable audio only streams in the streams manifest.");
        }

        return await youTubeClient.Videos.Streams.GetAsync(stream, cancellationToken);
    }
}