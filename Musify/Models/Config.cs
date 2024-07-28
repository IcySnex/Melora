using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Enums;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;

namespace Musify.Models;

public class Config
{
    public static readonly string ConfigFilepath = Path.Combine(Environment.CurrentDirectory, "Config.json");


    public Dictionary<string, IPluginConfig> PluginConfigs { get; set; } = [];

    public ConfigDownloads Downloads { get; set; } = new();

    public ConfigPaths Paths { get; set; } = new();

    public ConfigLyrics Lyrics { get; set; } = new();
}


public partial class ConfigLyrics : ObservableObject
{
    [ObservableProperty]
    string geniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";

    [ObservableProperty]
    Sorting searchResultsSorting = Sorting.Default;

    [ObservableProperty]
    bool searchResultsSortDescending = false;
}

public partial class ConfigDownloads : ObservableObject
{
    [ObservableProperty]
    AlreadyExistsBehavior alreadyExistsBehavior = AlreadyExistsBehavior.Ask;

    [ObservableProperty]
    Sorting sorting = Sorting.Default;

    [ObservableProperty]
    bool sortDescending = false;
}

public partial class ConfigPaths : ObservableObject
{
    [ObservableProperty]
    string downloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    [ObservableProperty]
    string filename = "{artist} - {title}";

    [ObservableProperty]
    string fFMPEGLocation = "FFMPEG.exe";
}