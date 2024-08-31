---
title: Platform-Support
icon: fe:music
order: 5
---

This guide will walk you through the process of creating your own Melora Platform-Support plugins by using the integration of SoundCloud with Melora as an example.
It is easily adaptable to any other platform as well though.

If you're new to Platform-Support plugins, please refer to the [Beginner's Guide](/Melora/guide/platform-support.html) for an introduction.


## Setup
Before diving into coding your new shiny Melora plugin, it's important to first follow the plugin development [Getting Started Guide](/Melora/plugin-development/getting-started.html). This guide will walk you through setting up your IDE, project, and plugin bundle manifest, ensuring you're fully prepared to start developing your plugin.


## Structure
Before you begin, it's **helpful** to organize your plugin bundle with the following structure:
```
└─ Melora.PlatformSupport.<NAME>
   ├─ Internal
   │  └─ ...
   ├─ Manifest.json
   └─ <NAME>Plugin.cs
```
- **Root:** It's recommended to place all plugins *(classes that implement IPlugin)* in the **root** of your plugin bundle. The manifest should also be here.
- **Internal:** Any additional components *(like models, helper classes...)* required internally should be organized within an **internal** namespace.
This structure keeps your plugin **organized** and ensures that your main functionalities are **easily accessible** while keeping auxiliary components **neatly separated**.


## Implementing Your Plugin

### 1. The Manifest
To ensure Melora understands what your plugin bundle does, you first need to provide some essential information via a **manifest file**.
This file tells Melora about your plugin’s purpose, author, and more. For instructions on adding the manifest to your project, refer to the [Getting Started Guide](/Melora/plugin-development/getting-started.html#create-a-plugin-manifest).

#### Example:
```json
{
  "Name": "SoundCloud",
  "Description": "Adds support to search & download tracks, sets & users from SoundCloud.",
  "Author": "IcySnex",
  "ApiVersion": "1.0.0",
  "LastUpdatedAt": "2024-08-28T14:26:00Z",
  "SourceUrl": "https://github.com/IcySnex/Melora/tree/main/Plugins/Melora.PlatformSupport.SoundCloud",
  "DownloadUrl": "https://github.com/IcySnex/Melora/releases/download/v1.0.0-stable/Melora.PlatformSupport.SoundCloud.mlr",
  "PluginKinds": [
    0
  ],
  "EntryPoint": "Melora.PlatformSupport.SoundCloud.dll",
  "Dependencies": [
    "SoundCloudExplode.dll"
  ]
}
```

### 2. The Plugin Class
- Create a new class called `<NAME>Plugin.cs` in the root of your project.
- This class should inherit from `PlatformSupportPlugin`, which itself implements the `IPlugin` interface.
```cs
public class SoundCloudPlugin : PlatformSupportPlugin
{
    public override async Task<IEnumerable<SearchResult>> SearchAsync(
        string query,
        IProgress<string> progress,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public override Task<IEnumerable<DownloadableTrack>> PrepareDownloadsAsync(
        IEnumerable<SearchResult> searchResults,
        IProgress<string> progress,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public override Task<Stream> GetStreamAsync(
        DownloadableTrack track,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
```

#### Methods:
| Method | Description |
| --- | --- |
| `SearchAsync` | Handles searching based on the query input by the user. |
| `PrepareDownloadsAsync` | This method will convert the selected `SearchResult`'s to proper `DownloadableTrack`'s. |
| `GetStreamAsync` | Responsible for downloading the track and returning an audio stream. The stream's format doesn't matter, as it will be re-encoded later on. |

### 3. The Constructors
The `PlatformSupportPlugin` class **requires** a few key pieces of information from your plugin, which are passed through its constructor. Here’s a breakdown of the parameters:
| Parameter | Description |
| --- | --- |
| `string` name | The name of the plugin. This will be used to display your plugin in the Melora client UI. |
| `string` iconPathData | The svg path data for the plugin icon. This will be used to display your plugin in the Melora client UI. |
| `PlatformSupportPluginConfig` config | The config the plugin gets initialized with. Learn more about plugin configs [here](/Melora/plugin-development/configs.html). |
| `ILogger<IPlugin>?` logger | An optional logger. |

- Your plugin's constructor will need to pass these parameters when calling the **base class constructor** to properly initialize the `PlatformSupportPlugin`.

#### Example:
The Melora client will attempt to use various constructors based on your implementation and if a config is already saved for your plugin. Thats why you need to implement multiple constructors.

- Here are the required constructors if you plan on using logging in your plugin:
```cs
public SoundCloudPlugin(PlatformSupportPluginConfig? config, ILogger<IPlugin> logger) : base(
    name: "SoundCloud",
    iconPathData: "M219.8 390.3V133.6c0-8.2 2.5-13.1 7.4-14.6 82.2-19.4 165.3 37.9 172.9 124.4 80.4-33.9 150.3 68.4 88 129.8-15.8 15.6-34.7 23.4-56.8 23.4l-207.2-.2c-2.8-1.1-4.3-3.9-4.3-6.1h0zm-55.7-7.5c0 18.5 35.3 18.8 35.3 0v-247c0-23.4-35.3-23.3-35.3 0v247h0zm-54.8 0c0 18.1 35.2 18.8 35.2 0V237.2c0-23.4-35.2-23.3-35.2 0v145.6h0zm-54.5-7.6c0 18.7 35 19 35 0V215.7c0-22.6-35-22.9-35 0v159.5zM0 346c0 21.8 35 27.3 35 0v-68.4c0-23.2-35-23-35 0V346z",
    config: new(
        defaultItems:
        [
            new("Save Lyrics", "Whether to search & save lyrics", true),
            new("Client ID", "The SoundCloud API client ID", "")
        ],
        defaultQuality: Quality._160kbps,
        defaultFormat: Format.mp3,
        defaultSearchResultsLimit: null,
        defaultSearchResultsSorting: Sorting.Default,
        defaultSearchResultsSortDescending: false,
        initialConfig: config),
    logger: logger)
{ }

public SoundCloudPlugin(ILogger<IPlugin> logger) : this(null, logger)
{ }
```

- If you don’t need logging in your plugin, use these constructors:
```cs
public SoundCloudPlugin(PlatformSupportPluginConfig? config) : base(
    name: "SoundCloud",
    iconPathData: "M219.8 390.3V133.6c0-8.2 2.5-13.1 7.4-14.6 82.2-19.4 165.3 37.9 172.9 124.4 80.4-33.9 150.3 68.4 88 129.8-15.8 15.6-34.7 23.4-56.8 23.4l-207.2-.2c-2.8-1.1-4.3-3.9-4.3-6.1h0zm-55.7-7.5c0 18.5 35.3 18.8 35.3 0v-247c0-23.4-35.3-23.3-35.3 0v247h0zm-54.8 0c0 18.1 35.2 18.8 35.2 0V237.2c0-23.4-35.2-23.3-35.2 0v145.6h0zm-54.5-7.6c0 18.7 35 19 35 0V215.7c0-22.6-35-22.9-35 0v159.5zM0 346c0 21.8 35 27.3 35 0v-68.4c0-23.2-35-23-35 0V346z",
    config: new(
        defaultItems:
        [
            new("Save Description", "Whether to save the description as the lyrics", true),
            new("Client ID", "The SoundCloud API client ID", "")
        ],
        defaultQuality: Quality._160kbps,
        defaultFormat: Format.mp3,
        defaultSearchResultsLimit: null,
        defaultSearchResultsSorting: Sorting.Default,
        defaultSearchResultsSortDescending: false,
        initialConfig: config),
    logger: null)
{ }

public SoundCloudPlugin() : this(null)
{ }
```
::: warning Note
It is **always recommended** to use logging in your plugin. This makes debugging **easier** if a user ever encounters any issues.

Especially since it's very easy to implement logging with Melora's plugin infrastructure.
:::

### 4. Validate
Before proceeding, ensure that everything is set up correctly. If you have followed the [Getting Started Guide]("/Melora/plugin-development/getting-started.html"), you should be able to validate your plugin as follows:
- **Start Melora:** In Visual Studio, press the **"▶ Melora (Unpackaged)"** button. This will **launch** Melora with your plugin automatically loaded.
- You should see a notification in the **bottom right corner** of Melora indicating that your plugin has been loaded.
- On the right side of the Melora client, in the NavigationView, your plugin, including its icon, should be visible.

![](/plugin-development/platformsupport1.webp)

### 5. Searching
Once your plugin is set up and you have validated it loads in Melora, you can finally start coding the search functionality for your platform-support plugin.

The `PlatformSupportPlugin` class requires you to override the `SearchAsync()` method. This method provides:
| Parameter | Description |
| --- | --- |
| `string` query | The search query entered by the user. |
| `IProgress<string>` progress | An object to report progress which will be shown in the Melora client UI. |
| `CancellationToken` cancellationToken | A token to handle cancellation requests. |

#### High-Level Overview:
- **Implement the SearchAsync() Method:** Write the logic to perform searches specific to your platform, such as SoundCloud. You’ll need to integrate with the platform's API to fetch search results.
- **Progress Reporting:** Use the progress parameter to update the UI with search progress, enhancing user experience.
- **Cancellation Handling:** Ensure that your search operation can be cancelled using the cancellationToken if the user decides to stop the search.

::: info Note
For better organization and clarity, it’s recommended to create a "Wrapper" class in the **Internal namespace**. This class can handle all platform-specific interactions, keeping your `SearchAsync()` method focused on integrating with your wrapper.
:::

#### What to Return:
Each platform may return different result types, like `Track` models from Spotify or `Video` models from YouTube. In Melora, you need to convert these models to the `SearchResult` model to handle everything uniformly. This also benefits memory usage and performance, as it avoids unnecessary processing of tracks that the user may not end up downloading.

You can use the `Melora.Plugins` API’s static method in the `SearchResult` class to convert and buffer `IAsyncEnumerable<T>` to `IEnumerable<SearchResult>`:
```cs
public static async Task<IEnumerable<SearchResult>> BufferAsync<T>(
    IAsyncEnumerable<T> source,
    int limit,
    Func<T, int, SearchResult?> createSearchResult,
    CancellationToken cancellationToken)
```

If your platform API doesn’t use `IAsyncEnumerable`, you can use the LINQ `Select` method to achieve similar results:
```cs
public static IEnumerable<TResult> Select<TSource, TResult>(
    this IEnumerable<TSource> source,
    Func<TSource, int, TResult> selector)
```

---

#### Example:
You can see the entire source code *- including the wrapper -* on [GitHub](https://github.com/IcySnex/Melora/tree/main/Plugins/Melora.PlatformSupport.SoundCloud/).
```cs
public async Task<IEnumerable<SearchResult>> SearchAsync(
    string query,
    IProgress<string> progress,
    CancellationToken cancellationToken)
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
```

#### Validate:
If you did everything correct, you should now be able to use the Melora UI to search seamlessly on your platform.

![](/plugin-development/platformsupport2.webp)

### 6. Preparing For Download
In the previous step, you returned a `SearchResult` model, which **only** contains the **basic information** needed to display tracks in the Melora client UI. However, when the user decides to download a track, you’ll need to convert this `SearchResult` into a `DownloadableTrack`. This conversion allows you to **fetch additional information**, such as lyrics, genres, or any other data you may want to add.

This approach offers **significant** performance benefits. Instead of gathering all possible information during the search phase *- which might slow down the process —* you're only fetching detailed data when it's truly needed, i.e., when the user selects a track for download.

::: info Note
Since you may have to process a lot of tracks, using **parallelism** is suggested to decrease the amount of time the user has to wait.
:::

#### Avoid Duplicate Requests:
The intermediate step of using `SearchResult` models is useful, but what if your initial search request **already contains** some the information you need for your `DownloadableTrack`? To avoid redundant requests, you can store **additional data** directly within the `SearchResult` model. This data can then be accessed later when converting the `SearchResult` to a `DownloadableTrack`, **preventing unnecessary** API calls.

Here’s how you might implement this in your search method:
```cs
SearchResult result = new(
    title: track.Name,
    artists: string.Join(", ", track.Artists.Select(artist => artist.Name)),
    duration: TimeSpan.FromMilliseconds(track.DurationMs),
    imageUrl: album.Artwork?.LowRes,
    id: track.Id,
    items: new()
    {
        { "ReleaseDate", album.ReleaseDate },
        { "AlbumName", album.Name },
        { "TrackNumber", track.TrackNumber },
        { "ArtworkFullRes", album.Artwork?.FullRes }
    });
```

When preparing the download, you can use the `GetItem<T>` method to easily access these stored items:
```cs
DateTime releaseDate = searchResult.GetItem<DateTime>("ReleaseDate");
string albumName = searchResult.GetItem<string>("AlbumName");
int trackNumber = searchResult.GetItem<int>("TrackNumber");
string? artworkFullRes = searchResult.GetItem<string?>("ArtworkFullRes");
```

#### Example
Since this example **doesn't need** to send additional requests, the parallel processing is wrapped in `Task.Run` to **avoid blocking** the UI. For an example that requires async preparing of `DownloadableTrack`'s, please take a look at [this example](https://github.com/IcySnex/Melora/blob/main/Plugins/Melora.PlatformSupport.Spotify/SpotifyPlugin.cs#L101-L126).
```cs
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
```

#### Validate:
If you did everything correct, you should now be able to use the **Download** button to move your selected search results to the download queue.

![](/plugin-development/platformsupport3.webp)

### 7. Downloading
The final step in completing your Platform-Support plugin is implementing the **download functionality**. For this, you'll need to override the `GetStreamAsync` method in your `PlatformSupportPlugin` class. This method is responsible for providing a stream of the audio data that the user wants to download.

This method should return a `Stream` containing the audio data for the given `DownloadableTrack`. The format of this stream (e.g., MP3, OGG) is not important, as the audio will be **reencoded** with [FFmpeg](https://www.ffmpeg.org/ffmpeg-codecs.html#Audio-Decoders) later.

#### Example:
Since the wrapper class handles the heavy lifting class in our little SoundCloud example, this method will just pass the track Id to the wrapper:
```cs
public override Task<Stream> GetStreamAsync(
    DownloadableTrack track,
    CancellationToken cancellationToken = default) =>
    wrapper.GetStreamAsync(track.Id, cancellationToken);
```


## Publishing
If you’ve followed the guide successfully, you should now be able to download **any** track from your platform seamlessly. Before you publish, make sure to test everything **thoroughly** to ensure that your plugin functions as expected.

Once you’ve **verified** that everything works **properly**, refer to the [Publishing Guide](/Melora/plugin-development/publishing.html) to list your plugin in the [official Melora Plugin Collection](/Melora/plugin-collection/platform-support.html).