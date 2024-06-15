using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Enums;
using Musify.Models;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;

namespace Musify.Services;

public class YouTubeMusic
{
    public static YouTubeMusicSearchType GetSearchType(
        string query,
        out string? id)
    {
        id = null;

        return YouTubeMusicSearchType.Query;
    }


    readonly ILogger<YouTubeMusic> logger;
    readonly Config config;

    readonly YouTubeMusicClient client;

    public YouTubeMusic(
        ILogger<YouTubeMusic> logger,
        IOptions<Config> config)
    {
        this.logger = logger;
        this.config = config.Value;

        client = new(logger);

        logger.LogInformation("[YouTubeMusic-.ctor] YouTubeMusic has been initialized");
    }


    public Task<IEnumerable<Song>> SearchQueryAsync(
        string query,
        CancellationToken cancellationToken = default!)
    {
        logger.LogInformation("[YouTubeMusic-SearchQueryAsync] Searching for query...");

        return client.SearchAsync<Song>(query, config.Advanced.YouTubeMusicHL, config.Advanced.YouTubeMusicGL, cancellationToken);
    }


    public async IAsyncEnumerable<Track> ConvertAsync(
        IEnumerable<Song> songs,
        string? album = null)
    {
        logger.LogInformation("[YouTubeMusic-ConvertAsync] Converting YouTube Music videos...");

        foreach (Song song in songs)
        {
            await Task.Delay(100);
            yield return new Track(
                song.Name,
                string.Join(", ", song.Artists.Select(artist => artist.Name)),
                song.Duration,
                song.Thumbnails.OrderBy(thumbnail => thumbnail.Width * thumbnail.Height).LastOrDefault()?.Url,
                album,
                Source.YouTubeMusic,
                Song.GetUrl(song));
        }
    }
}