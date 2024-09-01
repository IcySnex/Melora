using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Enums;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using System.Text.Json.Serialization;

namespace Melora.Models;

public class Config
{
    public static readonly string ConfigFilepath = Path.Combine(Environment.CurrentDirectory, "Config.json");


    [JsonConstructor]
    public Config(
        ConfigLyrics lyrics,
        ConfigDownloads downloads,
        ConfigPaths paths,
        ConfigUpdates updates,
        ConfigPluginBundles pluginBundles)
    {
        Lyrics = lyrics;
        Downloads = downloads;
        Paths = paths;
        Updates = updates;
        PluginBundles = pluginBundles;
    }

    public Config() =>
        Reset();


    public ConfigLyrics Lyrics { get; } = new();

    public ConfigDownloads Downloads { get; } = new();

    public ConfigPaths Paths { get; } = new();

    public ConfigUpdates Updates { get; } = new();

    public ConfigPluginBundles PluginBundles { get; } = new();


    public void Reset()
    {
        Lyrics.GeniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";
        Lyrics.SearchResultsSorting = Sorting.Default;
        Lyrics.SearchResultsSortDescending = false;

        Downloads.SelectedMetadatePlugin = null;
        Downloads.AlreadyExistsBehavior = AlreadyExistsBehavior.Ask;
        Downloads.Sorting = Sorting.Default;
        Downloads.SortDescending = false;

        Paths.DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        Paths.Filename = "{artists} - {title}";
        Paths.FFmpegLocation = Path.Combine(Environment.CurrentDirectory, "FFmpeg.exe");

        Updates.ReleasesUrl = "https://api.github.com/repos/IcySnex/Melora/releases";
        Updates.Channel = UpdateChannel.Stable;
        Updates.AutomaticUpdateCheck = true;

        //PluginBundles.ShowInstalled = true;
        //PluginBundles.ShowAvailable = true;
        PluginBundles.ShowOfKindPlatformSupport = true;
        PluginBundles.ShowOfKindMetadata = true;
        PluginBundles.Sorting = Sorting.Default;
        PluginBundles.SortDescending = false;
    }
}


public partial class ConfigLyrics : ObservableObject
{
    [ObservableProperty]
    string geniusAccessToken = default!;

    [ObservableProperty]
    Sorting searchResultsSorting = default!;

    [ObservableProperty]
    bool searchResultsSortDescending = default!;
}

public partial class ConfigDownloads : ObservableObject
{
    [ObservableProperty]
    string? selectedMetadatePlugin = null;

    [ObservableProperty]
    AlreadyExistsBehavior alreadyExistsBehavior = default!;

    [ObservableProperty]
    Sorting sorting = default!;

    [ObservableProperty]
    bool sortDescending = default!;
}

public partial class ConfigPaths : ObservableObject
{
    [ObservableProperty]
    string downloadLocation = default!;

    [ObservableProperty]
    string filename = default!;

    [ObservableProperty]
    string fFmpegLocation = default!;
}

public partial class ConfigUpdates : ObservableObject
{
    [ObservableProperty]
    string releasesUrl = default!;

    [ObservableProperty]
    UpdateChannel channel = default!;

    [ObservableProperty]
    bool automaticUpdateCheck = default!;
}

public partial class ConfigPluginBundles : ObservableObject
{
    public Dictionary<string, IPluginConfig> Configs { get; set; } = [];

    //[ObservableProperty]
    //bool showInstalled = default!;

    //[ObservableProperty]
    //bool showAvailable = default!;

    [ObservableProperty]
    bool showOfKindPlatformSupport = default!;

    [ObservableProperty]
    bool showOfKindMetadata = default!;

    [ObservableProperty]
    Sorting sorting = default!;

    [ObservableProperty]
    bool sortDescending = default!;
}