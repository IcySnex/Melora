using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Enums;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;

namespace Melora.Models;

public class Config
{
    public static readonly string ConfigFilepath = Path.Combine(Environment.CurrentDirectory, "Config.json");


    public Config()
    {
        Reset();
    }


    public ConfigDownloads Downloads { get; set; } = new();

    public ConfigPaths Paths { get; set; } = new();

    public ConfigLyrics Lyrics { get; set; } = new();

    public ConfigPluginBundles PluginBundles { get; set; } = new();


    public void Reset()
    {
        Downloads.SelectedMetadatePlugin = null;
        Downloads.AlreadyExistsBehavior = AlreadyExistsBehavior.Ask;
        Downloads.Sorting = Sorting.Default;
        Downloads.SortDescending = false;

        Lyrics.GeniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";
        Lyrics.SearchResultsSorting = Sorting.Default;
        Lyrics.SearchResultsSortDescending = false;

        Paths.DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        Paths.Filename = "{artists} - {title}";
        Paths.FFmpegLocation = "FFmpeg.exe";

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