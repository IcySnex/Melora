using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using System.Collections.Generic;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace Musify.Services;

public class YouTube
{
    public static YouTubeSearchType GetSearchType(
        string query,
        out string? id)
    {
        if (VideoId.TryParse(query) is VideoId videoId)
        {
            id = videoId.Value;
            return YouTubeSearchType.Video;
        }
        if (PlaylistId.TryParse(query) is PlaylistId playlistId)
        {
            id = playlistId.Value;
            return YouTubeSearchType.Playlist;
        }
        if (ChannelId.TryParse(query) is ChannelId channelId)
        {
            id = channelId.Value;
            return YouTubeSearchType.Channel;
        }

        id = null;
        return YouTubeSearchType.Query;
    }


    readonly ILogger<YouTube> logger;
    readonly Config config;

    readonly YoutubeClient client;

    public YouTube(
        ILogger<YouTube> logger,
        IOptions<Config> config)
    {
        this.logger = logger;
        this.config = config.Value;

        client = new();

        logger.LogInformation("[YouTube-.ctor] YouTube has been initialized");
    }


    public async Task<IVideo> SearchVideoAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for video...");
        progress?.Report("Searching for video...");

        return await client.Videos.GetAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchPlaylistAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for playlist...");
        progress?.Report("Searching for playlist...");

        return client.Playlists.GetVideosAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchChannelAsync(
        string id,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for channel...");
        progress?.Report("Searching for channel...");

        return client.Channels.GetUploadsAsync(id, cancellationToken);
    }

    public IAsyncEnumerable<IVideo> SearchQueryAsync(
        string query,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTube-SearchTrackAsync] Searching for channel...");
        progress?.Report("Searching for channel...");

        return client.Search.GetVideosAsync(query, cancellationToken);
    }
}