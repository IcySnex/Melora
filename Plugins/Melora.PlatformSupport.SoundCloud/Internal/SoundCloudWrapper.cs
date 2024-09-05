using Melora.Plugins.Abstract;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using SoundCloudExplode;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Users;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Melora.PlatformSupport.SoundCloud.Internal;

internal partial class SoundCloudWrapper
{
    [GeneratedRegex(@"^https?:\/\/soundcloud\.com\/([a-zA-Z0-9_-]+(\/[a-zA-Z0-9_-]+(\/[a-zA-Z0-9_-]+)?)?)\/?(\?.*)?$")]
    private static partial Regex SoundCloudUrlRegex();

    private static Func<Track, int, SearchResult?> GetBufferFunc(
        IProgress<string> progress,
        string leftTracksToBuffer,
        string? setTitle,
        long? setTotalTracks) =>
        (Track track, int index) =>
        {
            progress.Report($"Buffering tracks {index}{leftTracksToBuffer}...");

            if (track.Title is null || track.User?.Username is null)
                return null;

            return new SearchResult(
                title: track.Title,
                artists: track.User.Username,
                duration: track.FullDuration is null ? TimeSpan.Zero : TimeSpan.FromMilliseconds((double)track.FullDuration),
                imageUrl: track.ArtworkUrl?.AbsoluteUri,
                id: track.Id.ToString(),
                items: new()
                {
                    { "DisplayDate", track.DisplayDate.DateTime },
                    { "SetTitle", setTitle },
                    { "Genre", track.Genre },
                    { "Description", track.Description },
                    { "SetTotalTracks", (int)setTotalTracks.GetValueOrDefault(0) },
                    { "TrackNumber", index },
                    { "Path", track.PermalinkUrl!.PathAndQuery[1..] }
                });
        };


    public static string? GetPath(
        string url)
    {
        Match match = SoundCloudUrlRegex().Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static SoundCloudSearchType GetSearchType(
        string query,
        out string? path)
    {
        path = null;
        if (GetPath(query) is not string urlId)
            return SoundCloudSearchType.Query;

        path = urlId;
        string[] segments = path.Split('/');

        if (segments.Length == 1)
            return SoundCloudSearchType.User;
        else if (segments.Length == 2)
            return SoundCloudSearchType.Track;
        else if (segments.Length >= 3 && segments[1].Equals("sets", StringComparison.OrdinalIgnoreCase))
            return SoundCloudSearchType.Set;

        throw new Exception("URL returned good path but is not a track/set/user."); // ???
    }


    readonly PlatformSupportPluginConfig config;
    readonly ILogger<IPlugin>? logger;

    SoundCloudClient client = default!;
    HttpClient httpClient = default!;

    public SoundCloudWrapper(
        PlatformSupportPluginConfig config,
        ILogger<IPlugin>? logger = null)
    {
        this.config = config;
        this.logger = logger;

        AuthenticateClient();
        AuthenticatsHttpClient();

        logger?.LogInformation("[SoundCloudWrapper-.ctor] SoundCloudWrapper has been initialized");
    }


    public async void AuthenticateClient()
    {
        string clientId = config.GetStringOption("Client ID");

        client = new(clientId);
        if (string.IsNullOrEmpty(clientId))
            await client.InitializeAsync();

        logger?.LogInformation("[SoundCloudWrapper-AuthenticateClient] Client has been authenticated.");
    }

    public void AuthenticatsHttpClient()
    {
        httpClient = new();

        logger?.LogInformation("[SoundCloudWrapper-AuthenticatsHttpClient] HTTP client has been authenticated.");
    }


    public async Task<SearchResult> SearchTrackAsync(
        string path,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudWrapper-SearchTrackAsync] Searching for track...");
        progress.Report("Searching for track...");

        Track? track = await client.Tracks.GetAsync($"https://soundcloud.com/{path}", cancellationToken);
        if (track is null || track.Title is null || track.User?.Username is null)
            throw new SoundcloudExplodeException($"A track with this path could not be found: {path}.");

        SearchResult result = new(
            title: track.Title,
            artists: track.User.Username,
            duration: track.FullDuration is null ? TimeSpan.Zero : TimeSpan.FromMilliseconds((double)track.FullDuration),
            imageUrl: track.ArtworkUrl?.AbsoluteUri,
            id: path,
            items: new()
            {
                { "DisplayDate", track.DisplayDate.DateTime },
                { "SetTitle", null },
                { "Genre", track.Genre },
                { "Description", track.Description },
                { "SetTotalTracks", 0 },
                { "TrackNumber", 0 },
                { "Path", path }
            });
        return result;
    }

    public async Task<IEnumerable<SearchResult>> SearchSetAsync(
        string path,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudWrapper-SearchSetAsync] Searching for set...");
        progress.Report("Searching for set...");

        Playlist set = await client.Playlists.GetAsync($"https://soundcloud.com/{path}", false, cancellationToken);
        IAsyncEnumerable<Track> tracks = client.Playlists.GetTracksAsync($"https://soundcloud.com/{path}", cancellationToken);

        int totalTracksToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), (int)set.TrackCount.GetValueOrDefault(0));
        string leftTracksToBuffer = totalTracksToBuffer != 0 ? $"/{totalTracksToBuffer}" : string.Empty;

        return await SearchResult.BufferAsync(
            tracks,
            totalTracksToBuffer,
            GetBufferFunc(progress, leftTracksToBuffer, set.Title, set.TrackCount),
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchUserAsync(
        string path,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudWrapper-SearchSetAsync] Searching for user...");
        progress.Report("Searching for user...");

        User user = await client.Users.GetAsync($"https://soundcloud.com/{path}", cancellationToken);
        IAsyncEnumerable<Track> tracks = client.Users.GetPopularTracksAsync($"https://soundcloud.com/{path}", cancellationToken);

        int totalTracksToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), 50);
        string leftTracksToBuffer = totalTracksToBuffer != 0 ? $"/{totalTracksToBuffer}" : string.Empty;

        return await SearchResult.BufferAsync(
            tracks,
            totalTracksToBuffer,
            GetBufferFunc(progress, leftTracksToBuffer, $"{user.Username}'s Tracks", 0),
            cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchQueryAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudWrapper-SearchSetAsync] Searching for query...");
        progress.Report("Searching for query...");

        IAsyncEnumerable<Track> tracks = client.Search.GetTracksAsync(query, cancellationToken);

        int totalTracksToBuffer = Math.Min(config.SearchResultsLimit.GetValueOrDefault(int.MaxValue), 50);
        string leftTracksToBuffer = totalTracksToBuffer != 0 ? $"/{totalTracksToBuffer}" : string.Empty;

        return await SearchResult.BufferAsync(
            tracks,
            totalTracksToBuffer,
            GetBufferFunc(progress, leftTracksToBuffer, null, null),
            cancellationToken);
    }



    public DownloadableTrack PrepareDownload(
        SearchResult searchResult)
    {
        logger?.LogInformation("[SpotifyWrapper-PrepareDownloadAsync] Preparing track '{id}' for download...", searchResult.Id);

        bool saveDescription = config.GetBoolOption("Save Description");

        DateTime displayDate = searchResult.GetItem<DateTime>("DisplayDate");
        string? setTitle = searchResult.GetItem<string?>("SetTitle");
        string? genre = searchResult.GetItem<string?>("Genre");
        string? description = searchResult.GetItem<string?>("Description");
        int trackNumber = searchResult.GetItem<int>("TrackNumber");
        int setTotalTracks = searchResult.GetItem<int>("SetTotalTracks");
        string path = searchResult.GetItem<string>("Path");

        return new DownloadableTrack(
            title: searchResult.Title,
            artists: searchResult.Artists,
            duration: searchResult.Duration,
            artworkUrl: searchResult.ImageUrl?.Replace("-large.jpg", "-t500x500.jpg"),
            isExplicit: false,
            releasedAt: displayDate,
            album: setTitle,
            genre: genre,
            lyrics: saveDescription ? description : null,
            trackNumber: trackNumber,
            totalTracks: setTotalTracks,
            copyright: $"{searchResult.Title} - {searchResult.Artists} ℗ {displayDate.Year}, Provided to SoundCloud, Auto-generated by Melora.PlatformSupport.SoundCloud.SoundCloudPlugin",
            comment: "Downloaded with Melora, Melora.PlatformSupport.SoundCloud.SoundCloudPlugin",
            url: $"https://soundcloud.com/{path}",
            id: path);
    }


    public async Task<Stream> GetStreamAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudWrapper-GetStreamAsync] Getting track stream...");

        string streamUrl = await client.Tracks.GetDownloadUrlAsync($"https://soundcloud.com/{path}", cancellationToken)
            ?? throw new Exception($"SoundCloud did not return a stream URL.");

        return await httpClient.GetStreamAsync(streamUrl, cancellationToken);
    }
}