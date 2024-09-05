using Melora.PlatformSupport.SoundCloud.Internal;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace Melora.PlatformSupport.SoundCloud;

public class SoundCloudPlugin : PlatformSupportPlugin
{
    readonly SoundCloudWrapper wrapper;

    public SoundCloudPlugin(PlatformSupportPluginConfig? config, ILogger<IPlugin> logger) : base(
        name: "SoundCloud",
        iconPathData: "M219.8 390.3V133.6c0-8.2 2.5-13.1 7.4-14.6 82.2-19.4 165.3 37.9 172.9 124.4 80.4-33.9 150.3 68.4 88 129.8-15.8 15.6-34.7 23.4-56.8 23.4l-207.2-.2c-2.8-1.1-4.3-3.9-4.3-6.1h0zm-55.7-7.5c0 18.5 35.3 18.8 35.3 0v-247c0-23.4-35.3-23.3-35.3 0v247h0zm-54.8 0c0 18.1 35.2 18.8 35.2 0V237.2c0-23.4-35.2-23.3-35.2 0v145.6h0zm-54.5-7.6c0 18.7 35 19 35 0V215.7c0-22.6-35-22.9-35 0v159.5zM0 346c0 21.8 35 27.3 35 0v-68.4c0-23.2-35-23-35 0V346z",
        config: new(
            defaultOptions:
            [
                new BoolOption("Save Description", "Whether to save the description as the lyrics", true),
                new StringOption("Client ID", "The SoundCloud API client ID", "", 50, true)
            ],
            defaultQuality: Quality._160kbps,
            defaultFormat: Format.mp3,
            defaultSearchResultsLimit: null,
            defaultSearchResultsSorting: Sorting.Default,
            defaultSearchResultsSortDescending: false,
            initialConfig: config),
        logger: logger)
    {
        wrapper = new(Config, logger);

        Config.PropertyChanged += OnConfigPropertyChanged;
    }

    public SoundCloudPlugin(ILogger<IPlugin> logger) : this(null, logger)
    { }


    void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Client ID":
                wrapper.AuthenticateClient();
                break;
        }
    }


    public override async Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SoundCloudPlugin-SearchAsync] Getting SoundCloud search type...");
        progress.Report("Preparing search...");

        SoundCloudSearchType type = SoundCloudWrapper.GetSearchType(query, out string? path);
        switch (type)
        {
            case SoundCloudSearchType.Track:
                SearchResult trackResult = await wrapper.SearchTrackAsync(path!, progress, cancellationToken);
                return [trackResult];
            case SoundCloudSearchType.Set:
                IEnumerable<SearchResult> albumResults = await wrapper.SearchSetAsync(path!, progress, cancellationToken);
                return albumResults;
            case SoundCloudSearchType.User:
                IEnumerable<SearchResult> artistResults = await wrapper.SearchUserAsync(path!, progress, cancellationToken);
                return artistResults;
            case SoundCloudSearchType.Query:
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
        logger?.LogInformation("[SoundCloudPlugin-PrepareDownloadsAsync] Preparing search results for download...");

        SearchResult[] indexableSearchResults = searchResults.ToArray();
        DownloadableTrack[] results = new DownloadableTrack[indexableSearchResults.Length];

        await Task.Run(() =>
            Parallel.For(
                0, indexableSearchResults.Length,
                index =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress.Report($"Preparing downloads [{index}/{indexableSearchResults.Length}]...");

                    DownloadableTrack track = wrapper.PrepareDownload(indexableSearchResults[index]);
                    lock (results)
                        results[index] = track;
                }),
            cancellationToken);

        return results;
    }

    public override Task<Stream> GetStreamAsync(
        DownloadableTrack track,
        CancellationToken cancellationToken = default) =>
        wrapper.GetStreamAsync(track.Id, cancellationToken);
}