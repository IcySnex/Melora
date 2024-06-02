using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Enums;

namespace Musify.Models;

public class Config
{
    public ConfigSpotify Spotify { get; set; } = new();

    public ConfigYouTube YouTube { get; set; } = new();

    public ConfigDownloads Downloads { get; set; } = new();

    public ConfigPaths Paths { get; set; } = new();

    public ConfigAdvanced Advanced { get; set; } = new();
}


public partial class ConfigSpotify : ObservableObject
{
    [ObservableProperty]
    Quality quality = Quality._160kbps;

    [ObservableProperty]
    Format format = Format.mp3;

    [ObservableProperty]
    bool saveLyrics = true;

    [ObservableProperty]
    bool saveArtwork = true;

    public ViewOptions ViewOptions { get; set; } = new(Sorting.Default, false, 1000);
}

public partial class ConfigYouTube : ObservableObject
{
    [ObservableProperty]
    Quality quality = Quality._160kbps;

    [ObservableProperty]
    Format format = Format.mp3;

    [ObservableProperty]
    bool saveDescription = false;

    [ObservableProperty]
    bool saveThumbnail = true;

    public ViewOptions ViewOptions { get; set; } = new(Sorting.Default, false, 1000);
}

public partial class ConfigDownloads : ObservableObject
{
    [ObservableProperty]
    AlreadyExistsBehavior alreadyExistsBehavior = AlreadyExistsBehavior.Ask;

    [ObservableProperty]
    bool writeCurrentDateTimeAsRelease = false;

    [ObservableProperty]
    bool showSpotifyTracks = true;

    [ObservableProperty]
    bool showYouTubeTracks = true;

    public ViewOptions ViewOptions { get; set; } = new(Sorting.Default, false, default);
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

public partial class ConfigAdvanced : ObservableObject
{
    [ObservableProperty]
    string spotifyYouTubeSearchAlgorithm = "{title} {artist}";

    [ObservableProperty]
    string spotifySearchMarket = "US"; // https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2

    [ObservableProperty]
    string spotifyClientId = "75e1749b48dd4466858cf28ab32b1c8a";

    [ObservableProperty]
    string spotifyClientSecret = "b884202c63af4bcbbcac91cfcf16e6c8";

    [ObservableProperty]
    string geniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";
}