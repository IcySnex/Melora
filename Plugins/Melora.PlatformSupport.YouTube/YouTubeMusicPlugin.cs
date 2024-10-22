﻿using Melora.PlatformSupport.YouTube.Internal;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Melora.PlatformSupport.YouTube;

public class YouTubeMusicPlugin : PlatformSupportPlugin
{
    readonly YouTubeMusicWrapper wrapper;

    public YouTubeMusicPlugin(
        PlatformSupportPluginConfig? config,
        ILogger<IPlugin>? logger = null) : base(
            "YouTube Music",
            "M236.1 512c-36.8-3.5-64.6-11.3-94.7-26.5-25.4-12.7-46.3-27.8-66.6-48-41.9-42-67.6-95.7-73.9-155.2-1.5-14.4-1.1-46.3.8-59.6C11.9 150.7 49.8 88 108.4 46.4c23.5-16.6 54.3-31 81.7-38C213.7 2.3 225.4.7 249.3.1c36.3-.9 66.2 4.1 98.5 16.5C429 47.6 489 117.9 507.1 203.1c4.2 19.9 4.9 27.6 4.9 53.1 0 18.3-.3 25.6-1.3 33.1-8.2 58-33.2 108.2-73.6 148.5-42.8 42.6-98 68.4-158.2 73.9-7.8.7-36.4.9-42.8.3zm44.3-39.1c51-5.9 96.6-28.2 132.1-64.7C452.4 367 474 313.8 474 256.3S452.7 146 412.6 104.4c-31.2-32.3-70.9-53.7-116-62.5-23.9-4.7-55.6-4.7-80.4-.1-74.2 14-135.9 65.1-163.3 135.5-14.9 38.2-18.6 82.9-10.2 123.2 9 43.2 30.2 81.7 61.6 112 35.6 34.3 77.8 54.4 127.3 60.4 11.4 1.3 36.7 1.3 48.8 0zm-99.8-89.2a19.51 19.51 0 0 1-13.8-14.6c-.5-2.8-.8-38.3-.6-115.2l1.5-114c4.8-10.5 16.7-14.8 26.8-9.6 2.3 1.2 47 26.4 99.3 56 101.3 57.3 100.2 56.6 102.3 63.6 1.2 3.9 1.2 8.9 0 12.8-2.1 6.9-1 6.2-102.3 63.6L194.9 382c-4.9 2.3-9.7 3-14.3 1.7zm91.9-89.8l66-37.7c0-.5-131.5-75-132.9-75.3-.8-.2-1 12.7-1 75.3l1 75.3c.6-.2 30.7-17.1 66.9-37.6z",
            new(
                defaultOptions:
                [
                    //new BoolOption("Save Lyrics", "Whether to search & save lyrics from Genius automatically", true),
                    new BoolOption("Playlist As Album", "Whether to set the playlist name as the album if possible", false),
                    new StringOption("Geographical Location", "The region for the YouTube Music search payload", "US", 2),
                ],
                defaultQuality: Quality._160kbps,
                defaultFormat: Format.mp3,
                defaultSearchResultsLimit: null,
                defaultSearchResultsSorting: Sorting.Default,
                defaultSearchResultsSortDescending: false,
                initialConfig: config),
            logger)
    {
        wrapper = new(Config, logger);

        Config.PropertyChanged += OnConfigPropertyChanged;
    }

    public YouTubeMusicPlugin(
        ILogger<IPlugin>? logger) : this(null, logger)
    { }


    void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Geographical Location":
                wrapper.AuthenticateClient();
                break;
        }
    }


    public override async Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SpotifyPlugin-SearchAsync] Getting Spotify search type...");
        progress.Report("Preparing search...");

        YouTubeMusicSearchType type = YouTubeMusicWrapper.GetSearchType(query, out string? id);
        switch (type)
        {
            case YouTubeMusicSearchType.SongVideo:
                SearchResult songVideoResult = await wrapper.SearchSongVideoAsync(id!, progress, cancellationToken);
                return [songVideoResult];
            case YouTubeMusicSearchType.Album:
                IEnumerable<SearchResult> albumResults = await wrapper.SearchAlbumAsync(id!, progress, cancellationToken);
                return albumResults;
            case YouTubeMusicSearchType.CommunityPlaylist:
                IEnumerable<SearchResult> communityPlaylistResults = await wrapper.SearchCommunityPlaylistAsync(id!, progress, cancellationToken);
                return communityPlaylistResults;
            case YouTubeMusicSearchType.Artist:
                IEnumerable<SearchResult> artistResults = await wrapper.SearchArtistAsync(id!, progress, cancellationToken);
                return artistResults;
            case YouTubeMusicSearchType.Query:
                IEnumerable<SearchResult> querytResults = await wrapper.SearchQueryAsync(query, progress, cancellationToken);
                return querytResults;
        }

        return [];
    }

    public override async Task<IEnumerable<DownloadableTrack>> PrepareDownloadsAsync(
        IEnumerable<SearchResult> searchResults,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubePlugin-PrepareDownloadsAsync] Preparing search results for download...");

        SearchResult[] indexableSearchResults = searchResults.ToArray();
        DownloadableTrack[] results = new DownloadableTrack[indexableSearchResults.Length];

        await Parallel.ForAsync(
            0, indexableSearchResults.Length,
            cancellationToken,
            async (index, token) =>
            {
                token.ThrowIfCancellationRequested();

                progress.Report($"Preparing downloads [{index}/{indexableSearchResults.Length}]...");

                DownloadableTrack track = await wrapper.PrepareDownloadAsync(indexableSearchResults[index], token);
                lock (results)
                    results[index] = track;
            });
        return results;
    }

    public override Task<Stream> GetStreamAsync(
        DownloadableTrack track,
        CancellationToken cancellationToken = default) =>
        wrapper.GetStreamAsync(track.Id, cancellationToken);
}