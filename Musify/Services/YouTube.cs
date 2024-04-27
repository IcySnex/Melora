using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Musify.Services;

public partial class YouTube
{
    [GeneratedRegex(@"(?:youtube\..+?/watch.*?v=|youtu\.be/|youtube\..+?/embed/|youtube\..+?/shorts/|youtube\..+?/live/)([^&/?]+)(?:&|/|\?si=[^&/]*)?")]
    private static partial Regex YouTubeVideoIdRegex();

    [GeneratedRegex(@"(?:youtube\..+?/playlist.*?list=|youtu\.be/.*?/.*?list=|youtube\..+?/watch.*?list=|youtube\..+?/embed/.*?/.*?list=)(.*?)(?:&|/|$)")]
    private static partial Regex YouTubePlaylistIdRegex();

    [GeneratedRegex(@"youtube\..+?/channel/(.*?)(?:\?|&|/|$)")]
    private static partial Regex YouTubeChannelIdRegex();


    public static bool IsValidVideoId(
        string videoId) =>
        videoId.Length == 11 && videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidPlaylistId(
        string playlistId) =>
        playlistId.Length >= 2 && playlistId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static bool IsValidChannelId(
        string channelId) =>
        channelId.StartsWith("UC", StringComparison.Ordinal) && channelId.Length == 24 && channelId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');


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

        id = null;
        return YouTubeSearchType.Query;
    }


    readonly ILogger<YouTube> logger;

    readonly YoutubeClient client;

    public YouTube(
        ILogger<YouTube> logger)
    {
        this.logger = logger;

        client = new();

        logger.LogInformation("[YouTube-.ctor] YouTube has been initialized");
    }


    public async Task<IVideo> SearchVideoAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for video...");
        return await client.Videos.GetAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchPlaylistAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for playlist...");
        return client.Playlists.GetVideosAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchChannelAsync(
        string id,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for channel...");
        return client.Channels.GetUploadsAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchQueryAsync(
        string query,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for channel...");
        return client.Search.GetVideosAsync(query, cancellationToken);
    }
}