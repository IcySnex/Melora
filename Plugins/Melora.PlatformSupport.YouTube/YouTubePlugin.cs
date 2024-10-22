﻿using Melora.PlatformSupport.YouTube.Internal;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;

namespace Melora.PlatformSupport.YouTube;

public class YouTubePlugin : PlatformSupportPlugin
{
    readonly YouTubeWrapper wrapper;

    public YouTubePlugin(
        PlatformSupportPluginConfig? config,
        ILogger<IPlugin>? logger) : base(
            "YouTube",
            "M205.2 442.5l-90.8-2.9c-46.8-2.7-69.8-10.2-87.8-28.8-14.7-15.1-21.4-32-25-62.7C.5 339.2.3 327.6.1 265.5-.3 192 .1 175 2.5 157.1c3.4-26.7 11.3-44.1 26.1-58.2 19.7-18.8 43.3-25.2 101.5-27.6 79.8-3.2 172.9-3.2 251.8 0 60 2.5 83.9 9.2 103.4 29.5 17 17.5 23.7 37.5 26 77.5.9 15.8.9 139.4 0 155.2-1.6 27.1-5.3 44.7-12.4 58.6-9 18-25.8 32.2-46.1 39-22.5 7.5-48.4 9.4-151.8 11.4-39.5.8-53.3.8-95.8 0zM336.1 406c77-2 95.8-3.9 112.1-11.8 18.2-8.8 24.9-23.9 27.4-61.5.9-13.9.9-139.8 0-153.8-2-29.7-6.2-43.6-16.2-53.6-9.4-9.4-23.2-14.1-49.6-16.5-58.4-5.4-249.3-5.4-307.7 0-46.3 4.3-59.7 16.1-64.7 56.6-1.1 8.6-1.3 19.4-1.5 81.4-.4 72.8.1 89.8 2.4 105.4 5.2 35.5 20.5 47.1 66.8 50.9 17.1 1.4 56.4 2.7 118 3.9 17.3.4 82.6-.2 113-1zm-121.6-85.8c-4.7-1.6-10-6.9-11.3-11.2-1.2-4.1-1.2-102.1 0-106.4.6-1.8 2.3-4.4 4.5-6.7 2.9-2.9 4.4-3.9 7.9-4.9 3.9-1.1 4.7-1.1 8.2-.2 5.9 1.6 87.9 48.4 92.4 52.8 7.3 7.2 7 18.3-.6 25.2-3.4 3.1-84.9 49.8-90 51.6-4.5 1.5-6.3 1.5-11.1-.2z",
            new(
                defaultOptions:
                [
                    new BoolOption("Save Description", "Whether to save the video description", false),
                    new BoolOption("Playlist As Album", "Whether to set the playlist name as the album if possible", true)
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
    }

    public YouTubePlugin(
        ILogger<IPlugin>? logger) : this(null, logger)
    { }


    public override async Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[YouTubePlugin-SearchAsync] Getting YouTube search type...");
        progress.Report("Preparing search...");

        YouTubeSearchType type = YouTubeWrapper.GetSearchType(query, out string? id);
        switch (type)
        {
            case YouTubeSearchType.Video:
                SearchResult videoResult = await wrapper.SearchVideoAsync(id!, progress, cancellationToken);
                return [videoResult];
            case YouTubeSearchType.Playlist:
                IEnumerable<SearchResult> playlistResults = await wrapper.SearchPlaylistAsync(id!, progress, cancellationToken);
                return playlistResults;
            case YouTubeSearchType.Channel:
                IEnumerable<SearchResult> channelResults = await wrapper.SearchChannelAsync(id!, progress, cancellationToken);
                return channelResults;
            case YouTubeSearchType.Query:
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