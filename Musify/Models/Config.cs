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


public class ConfigSpotify
{
    public Quality Quality { get; set; } = Quality._160kbps;

    public Format Format { get; set; } = Format.mp3;

    public bool SaveLyrics { get; set; } = true;

    public bool SaveArtwork { get; set; } = true;

    public Sorting SearchSorting { get; set; } = Sorting.Default;
}

public class ConfigYouTube
{
    public Quality Quality { get; set; } = Quality._160kbps;

    public Format Format { get; set; } = Format.mp3;

    public bool SaveDescription { get; set; } = false;

    public bool SaveThumbnail { get; set; } = true;

    public Sorting SearchSorting { get; set; } = Sorting.Default;
}

public class ConfigDownloads
{
    public AlreadyExistsBehavior AlreadyExistsBehavior = AlreadyExistsBehavior.Ask;

    public bool WriteCurrentDateTimeAsRelease = false;

    public bool ShowSpotifyTracks = true;

    public bool ShowYouTubeTracks = true;
}

public class ConfigPaths
{
    public string DownloadLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    public string Filename { get; set; } = "{artist} - {title}";

    public string FFMPEGLocation { get; set; } = "FFMPEG.exe";
}

public class ConfigAdvanced
{
    public string SpotifyYouTubeSearchAlgorithm = "{title} {artist}";

    public string SpotifySearchMarket = "US"; // https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2

    public int SpotifyQuerySearchResultsLimit = 30; // Min: 30 - Max:50

    public string SpotifyClientId = "75e1749b48dd4466858cf28ab32b1c8a";

    public string SpotifyClientSecret = "b884202c63af4bcbbcac91cfcf16e6c8";

    public string GeniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";
}