using Melora.Plugins.Abstract;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Melora.PlatformSupport.YouTube.Internal;

internal partial class YouTubeWrapper
{
    [GeneratedRegex(@"(?:youtube\..+?/watch.*?v=|youtu\.be/|youtube\..+?/embed/|youtube\..+?/shorts/|youtube\..+?/live/)([^&/?]+)(?:&|/|\?si=[^&/]*)?")]
    private static partial Regex YouTubeVideoIdRegex();

    [GeneratedRegex(@"(?:youtube\..+?/playlist.*?list=|youtu\.be/.*?/.*?list=|youtube\..+?/watch.*?list=|youtube\..+?/embed/.*?/.*?list=)(.*?)(?:&|/|$)")]
    private static partial Regex YouTubePlaylistIdRegex();

    [GeneratedRegex(@"youtube\..+?/channel/(.*?)(?:\?|&|/|$)")]
    private static partial Regex YouTubeChannelIdRegex();
    [GeneratedRegex(@"youtube\..+?/(@[a-zA-Z0-9_]+)(?:\?|&|/|$)")]
    private static partial Regex YouTubeChannelHandleRegex();


    public static bool IsValidVideoId(
        string videoId) =>
        videoId.Length == 11 && videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidPlaylistId(
        string playlistId) =>
        playlistId.Length >= 2 && playlistId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidChannelId(
        string channelId) =>
        channelId.StartsWith("UC", StringComparison.Ordinal) && channelId.Length == 24 && channelId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');
    public static bool IsValidChannelHandle(
        string channelHandle) =>
        channelHandle.StartsWith('@') && channelHandle.Length > 3 && channelHandle.Length < 31 && channelHandle[1..].All(c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.');


    public static YouTubeSearchType GetSearchType(
        string query,
        out string? id)
    {
        Match videoIdMatch = YouTubeVideoIdRegex().Match(query);
        id = videoIdMatch.Success ? WebUtility.UrlDecode(videoIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidVideoId(id))
            return YouTubeSearchType.Video;

        Match playlistIdMatch = YouTubePlaylistIdRegex().Match(query);
        id = playlistIdMatch.Success ? WebUtility.UrlDecode(playlistIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidPlaylistId(id))
            return YouTubeSearchType.Playlist;

        Match channelIdMatch = YouTubeChannelIdRegex().Match(query);
        id = channelIdMatch.Success ? WebUtility.UrlDecode(channelIdMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidChannelId(id))
            return YouTubeSearchType.Channel;
        Match channelHandleMatch = YouTubeChannelHandleRegex().Match(query);
        id = channelHandleMatch.Success ? WebUtility.UrlDecode(channelHandleMatch.Groups[1].Value) : null;
        if (!string.IsNullOrWhiteSpace(id) && IsValidChannelHandle(id))
            return YouTubeSearchType.Channel;

        id = null;
        return YouTubeSearchType.Query;
    }


    public static string? GetLowResThumbnailUrl(
        IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.MinBy(thumbnail => (double)thumbnail.Resolution.Height / thumbnail.Resolution.Width == 0.75 ? int.MaxValue - thumbnail.Resolution.Area : thumbnail.Resolution.Area)?.Url;

    public static string? GetHighResThumbnailUrl(
        IEnumerable<Thumbnail> thumbnails) =>
        thumbnails.MaxBy(thumbnail => (double)thumbnail.Resolution.Height / thumbnail.Resolution.Width == 0.75 ? int.MinValue + thumbnail.Resolution.Area : thumbnail.Resolution.Area)?.Url;


    readonly PlatformSupportPluginConfig config;
    readonly ILogger<IPlugin>? logger;

    YoutubeClient client = default!;

    public YouTubeWrapper(
        PlatformSupportPluginConfig config,
        ILogger<IPlugin>? logger = null)
    {
        this.config = config;
        this.logger = logger;

        AuthenticateClient();

        logger?.LogInformation("[YouTubeWrapper-.ctor] YouTubeWrapper has been initialized");
    }


    public void AuthenticateClient()
    {
        client = new();

        logger?.LogInformation("[YouTubeWrapper-AuthenticateClient] Client has been authenticated.");
    }


    public async Task<SearchResult> SearchVideoAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeWrapper-SearchVideoAsync] Searching for video...");
        progress.Report("Searching for video...");

        Video video = await client.Videos.GetAsync(id, cancellationToken);

        SearchResult result = new(
            title: video.Title,
            artists: video.Author.ChannelTitle,
            duration: video.Duration.GetValueOrDefault(TimeSpan.Zero),
            imageUrl: GetLowResThumbnailUrl(video.Thumbnails),
            id: video.Id,
            items: new()
            {
                { "PlaylistName", null },
                { "VideoNumber", 0 },
                { "PlaylistTotalVideos", 0 }
            });
        return result;
    }

    public async Task<IEnumerable<SearchResult>> SearchPlaylistAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeWrapper-SearchPlaylistAsync] Searching for playlist...");
        progress.Report("Searching for playlist...");

        Playlist playlist = await client.Playlists.GetAsync(id, cancellationToken);
        IAsyncEnumerable<PlaylistVideo> videos = client.Playlists.GetVideosAsync(id, cancellationToken);

        int totalVideosToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), playlist.Count.GetValueOrDefault(0));
        string leftVideosToBuffer = totalVideosToBuffer != 0 ? $"/{totalVideosToBuffer}" : string.Empty;

        bool playlistAsAlbum = config.GetItem<bool>("Playlist As Album");

        return await SearchResult.BufferAsync(
            videos,
            totalVideosToBuffer,
            (PlaylistVideo video, int index) =>
            {
                progress.Report($"Buffering videos {index}{leftVideosToBuffer}...");

                return new SearchResult(
                    title: video.Title,
                    artists: video.Author.ChannelTitle,
                    duration: video.Duration.GetValueOrDefault(TimeSpan.Zero),
                    imageUrl: GetLowResThumbnailUrl(video.Thumbnails),
                    id: video.Id,
                    items: new()
                    {
                        { "PlaylistName", playlistAsAlbum ? playlist.Title : null },
                        { "VideoNumber", index + 1 },
                        { "PlaylistTotalVideos", playlistAsAlbum ? playlist.Count : 0 }
                    });
            },
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchChannelAsync(
        string id,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeWrapper-SearchChannelAsync] Searching for channel...");
        progress.Report("Searching for channel...");

        Channel channel = await client.Channels.GetAsync(id, cancellationToken);
        IAsyncEnumerable<PlaylistVideo> videos = client.Channels.GetUploadsAsync(id, cancellationToken);

        int totalVideosToBuffer = config.SearchResultsLimit.GetValueOrDefault(int.MaxValue);
        string leftVideosToBuffer = config.SearchResultsLimit.HasValue ? $"/{config.SearchResultsLimit}" : string.Empty;

        bool playlistAsAlbum = config.GetItem<bool>("Playlist As Album");

        return await SearchResult.BufferAsync(
            videos,
            totalVideosToBuffer,
            (PlaylistVideo video, int index) =>
            {
                progress.Report($"Buffering videos {index}{leftVideosToBuffer}...");

                return new SearchResult(
                    title: video.Title,
                    artists: video.Author.ChannelTitle,
                    duration: video.Duration.GetValueOrDefault(TimeSpan.Zero),
                    imageUrl: GetLowResThumbnailUrl(video.Thumbnails),
                    id: video.Id,
                    items: new()
                    {
                        { "PlaylistName", playlistAsAlbum ? $"{channel.Title.Replace(" - Topic", "")}'s Uploads" : null },
                        { "VideoNumber", index + 1 },
                        { "PlaylistTotalVideos", 0 }
                    });
            },
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchQueryAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeWrapper-SearchQueryAsync] Searching for query...");
        progress.Report("Searching for query...");

        IAsyncEnumerable<VideoSearchResult> videos = client.Search.GetVideosAsync(query, cancellationToken);

        int totalVideosToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), 50);
        string leftVideosToBuffer = config.SearchResultsLimit.HasValue ? $"/{config.SearchResultsLimit}" : $"/50";

        return await SearchResult.BufferAsync(
            videos,
            totalVideosToBuffer,
            (VideoSearchResult video, int index) =>
            {
                progress.Report($"Buffering videos {index}{leftVideosToBuffer}...");

                return new SearchResult(
                    title: video.Title,
                    artists: video.Author.ChannelTitle,
                    duration: video.Duration.GetValueOrDefault(TimeSpan.Zero),
                    imageUrl: GetLowResThumbnailUrl(video.Thumbnails),
                    id: video.Id,
                    items: new()
                    {
                        { "PlaylistName", null },
                        { "VideoNumber", 0 },
                        { "PlaylistTotalVideos", 0 }
                    });
            },
            cancellationToken);
    }


    public async Task<DownloadableTrack> PrepareDownloadAsync(
        SearchResult searchResult,
        CancellationToken cancellationToken = default)
    {
        bool saveDescription = config.GetItem<bool>("Save Description");

        logger?.LogInformation("[YouTubeWrapper-PrepareDownloadAsync] Getting video...");
        Video video = await client.Videos.GetAsync(searchResult.Id, cancellationToken);

        string? highresThumbnailUrl = GetHighResThumbnailUrl(video.Thumbnails);
        string? playlistName = searchResult.GetItem<string?>("PlaylistName");
        string? description = saveDescription ? video.Description : null;
        int videoNumber = searchResult.GetItem<int>("VideoNumber");
        int playlistTotalVideos = searchResult.GetItem<int>("PlaylistTotalVideos");

        return new DownloadableTrack(
            title: searchResult.Title,
            artists: searchResult.Artists,
            duration: searchResult.Duration,
            artworkUrl: highresThumbnailUrl,
            isExplicit: false,
            releasedAt: video.UploadDate.DateTime,
            album: playlistName,
            genre: null,
            lyrics: description,
            trackNumber: videoNumber,
            totalTracks: playlistTotalVideos,
            copyright: $"{searchResult.Title} - {searchResult.Artists} ℗ {video.UploadDate.Year}, Provided to YouTube, Auto-generated by Melora.PlatformSupport.YouTube.YouTubePlugin",
            comment: "Downloaded with Melora, Melora.PlatformSupport.YouTube.YouTubePlugin",
            url: $"https://www.youtube.com/watch?v={searchResult.Id}",
            id: searchResult.Id);
    }


    public async Task<Stream> GetStreamAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubeWrapper-GetStreamAsync] Getting video stream...");
        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(id, cancellationToken);

        AudioOnlyStreamInfo? stream = manifest
            .GetAudioOnlyStreams()
            .MinBy(streamInfo => Math.Abs((int)config.Quality - streamInfo.Bitrate.KiloBitsPerSecond))
            ?? throw new("Could not find any suitable audio only streams in the streams manifest.");

        return await client.Videos.Streams.GetAsync(stream, cancellationToken);
    }
}