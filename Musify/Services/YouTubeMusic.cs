using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using System.Net;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace Musify.Services;

public partial class YouTubeMusic
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



    readonly ILogger<YouTubeMusic> logger;

    readonly YouTubeMusicClient client;

    public YouTubeMusic(
        ILogger<YouTubeMusic> logger,
        IOptions<Config> config)
    {
        this.logger = logger;

        client = new(logger, config.Value.Advanced.YouTubeMusicHL, config.Value.Advanced.YouTubeMusicGL);

        logger.LogInformation("[YouTubeMusic-.ctor] YouTubeMusic has been initialized");
    }


    public async Task<CommunityPlaylistSongInfo> SearchSongVideoAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[YouTubeMusic-SearchSongVideoAsync] Searching for song/video...");

        SongVideoInfo info = await client.GetSongVideoInfoAsync(id, cancellationToken);
        return new(
            info.Name,
            info.Id,
            info.Artists,
            null,
            info.IsFamiliyFriendly,
            info.Duration,
            info.Thumbnails);
    }

    public async Task<IEnumerable<CommunityPlaylistSongInfo>> SearchAlbumAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTubeMusic-SearchAlbumAsync] Searching for album...");

        string browseId = await client.GetAlbumBrowseIdAsync(id, cancellationToken);
        AlbumInfo album = await client.GetAlbumInfoAsync(browseId, cancellationToken);

        ShelfItem albumItem = new(album.Name, album.Id, ShelfKind.Albums);

        return album.Songs.Select(song => new CommunityPlaylistSongInfo(
            song.Name,
            song.Id,
            album.Artists,
            albumItem,
            song.IsExplicit,
            song.Duration,
            album.Thumbnails));
    }
    
    public async Task<IEnumerable<CommunityPlaylistSongInfo>> SearchCommunityPlaylistAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTubeMusic-SearchCommunityPlaylistAsync] Searching for community playlist...");

        string browseId = client.GetCommunityPlaylistBrowseId(id);
        CommunityPlaylistInfo playlist = await client.GetCommunityPlaylistInfoAsync(browseId, cancellationToken);

        return playlist.Songs;
    }

    public async Task<IEnumerable<CommunityPlaylistSongInfo>> SearchArtistAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTubeMusic-SearchArtistAsync] Searching for artist...");

        ArtistInfo artist = await client.GetArtistInfoAsync(id, cancellationToken);

        if (artist.AllSongsPlaylistId is null)
            return [];

        return await SearchCommunityPlaylistAsync(artist.AllSongsPlaylistId, cancellationToken);
    }

    public async Task<IEnumerable<CommunityPlaylistSongInfo>> SearchQueryAsync(
        string query,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTubeMusic-SearchQueryAsync] Searching for query...");

        IEnumerable<Song> songs = await client.SearchAsync<Song>(query, cancellationToken);
        return songs.Select(song => new CommunityPlaylistSongInfo(
            song.Name,
            song.Id,
            song.Artists,
            song.Album,
            song.IsExplicit,
            song.Duration,
            song.Thumbnails));
    }


    public async IAsyncEnumerable<Track> ConvertAsync(
        IEnumerable<CommunityPlaylistSongInfo> songs)
    {
        logger.LogInformation("[YouTubeMusic-ConvertAsync] Converting YouTube Music videos...");

        foreach (CommunityPlaylistSongInfo song in songs)
        {
            await Task.Delay(50);
            yield return new Track(
                song.Name,
                string.Join(", ", song.Artists.Select(artist => artist.Name)),
                song.Duration,
                song.Thumbnails.OrderBy(thumbnail => thumbnail.Width * thumbnail.Height).LastOrDefault()?.Url,
                song.Album?.Name,
                Source.YouTubeMusic,
                $"https://music.youtube.com/watch?v={song.Id}");
        }
    }
}