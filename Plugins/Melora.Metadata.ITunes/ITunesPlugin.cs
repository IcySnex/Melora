using ATL;
using Melora.Metadata.ITunes.Internal;
using Melora.Plugins.Abstract;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace Melora.Metadata.ITunes;

public class ITunesPlugin : MetadataPlugin
{
    readonly PictureHandler pictureHandler;

    public ITunesPlugin(
        MetadataPluginConfig? config,
        ILogger<IPlugin>? logger) : base(
            "iTunes",
            "M312.8 371.2c-1.8.2-3.6.3-5.4.3-21.8 0-39.5-16.4-41.1-38.1-1.8-23.6 16-45.1 39.6-48.1 5.2-.7 10.5-.4 15.4.8v-94.4L229 209v128c0 .2 0 .5-.1.7v.1c1.7 23.5-16 45-39.6 48-1.8.2-3.6.3-5.4.3-21.8 0-39.5-16.4-41.1-38.1-1.8-23.6 15.9-45.1 39.6-48.1 5.2-.6 10.4-.3 15.4.9V155.1c0-2 1.5-3.8 3.5-4.1l146-24.9h.1c1.1-.2 2.3 0 3.2.6 1.3.8 2 2.3 1.9 3.8v.5 191.5.6.1c1.7 23.5-16.1 45.1-39.7 48zM256 0C114.8 0 0 114.8 0 256s114.8 256 256 256 256-114.8 256-256S397.2 0 256 0zm153.6 409.6c-41 41-95.6 63.6-153.6 63.6s-112.6-22.6-153.6-63.6S38.8 314 38.8 256s22.6-112.6 63.6-153.6S198 38.8 256 38.8s112.6 22.6 153.6 63.6S473.2 198 473.2 256s-22.6 112.6-63.6 153.6z",
            new(
                defaultOptions:
                [
                    new SelectableOption("Media Type", "The media type iTunes will recognize downloaded tracks as.", "Music", ["Music", "Audiobook", "Music Video", "Movie", "TV Show", "Booklet", "Ringtone",  "Podcast", "iTunes U"]),
                    new StringOption("Account", "The email address of the iTunes account used to 'purchase' downloaded track.", "", 25),
                    new StringOption("Owner", "The name of the account used to 'purchase' downloaded track.", "", 25),
                    new BoolOption("Current Date as Release", "Whether to set the current date as the release date.", false),
                    new BoolOption("Save Artwork", "Whether to save the artwork.", true),
                    new IntOption("Artwork Resolution", "The resolution (nXn) the artwork gets resized to before saving.", 512, 32, 1024)
                ],
                initialConfig: config),
            logger)
    {
        pictureHandler = new(Config, logger);
    }

    public ITunesPlugin(
        ILogger<IPlugin>? logger) : this(null, logger)
    { }


    [SupportedOSPlatform("windows")]
    public override async Task WriteAsync(
        string filePath,
        DownloadableTrack track,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[ITunesPlugin-WriteAsync] Starting to write track metadata...");

        Track file = new(filePath);
        if (file.AudioFormat.ShortName != "MPEG-4")
            throw new Exception("This Metadata Plugin only support writing audio files in 'MPEG-4' format.", new("Please select '4a' as the download format in your Platform-Support Plugin."));

        string mediaType = Config.GetSelectableOption("Media Type");
        string account = Config.GetStringOption("Account");
        string owner = Config.GetStringOption("Owner");
        bool currentDateAsRelease = Config.GetBoolOption("Current Date as Release");
        bool saveArtwork = Config.GetBoolOption("Save Artwork");

        // Default fields
        file.Title = track.Title;
        file.Artist = track.Artists.Replace(", ", "; ");
        file.Date = currentDateAsRelease ? DateTime.Now : track.ReleasedAt;
        file.Album = track.Album ?? string.Empty;
        file.AlbumArtist = track.Artists.Split(", ")[0];
        file.Genre = track.Genre ?? string.Empty;
        file.Lyrics.UnsynchronizedLyrics = track.Lyrics ?? string.Empty;
        file.TrackNumber = track.TrackNumber;
        file.TrackTotal = track.TotalTracks;
        file.Copyright = track.Copyright;
        file.Comment = track.Comment ?? string.Empty;
        file.AudioSourceUrl = track.Url;

        // Sorting
        file.SortTitle = file.Title;
        file.SortArtist = file.Artist;
        file.SortAlbum = file.Album;
        file.SortAlbumArtist = file.AlbumArtist;

        // Additional fields
        if (mediaType == "Podcast")
            file.AdditionalFields["pcst"] = "1";                                        // PODCAST
        else
            file.AdditionalFields["stik"] = mediaType switch                            // ITUNESMEDIATYPE 
            {
                "Music" => "1",
                "Audiobook" => "2",
                "Music Video" => "6",
                "Movie" => "9",
                "TV Show" => "10",
                "Booklet" => "11",
                "Ringtone" => "13",
                "iTunes U" => "23",
                _ => "1"
            };
        file.AdditionalFields["apID"] = account;                                        // ITUNESACCOUNT
        file.AdditionalFields["ownr"] = owner;                                          // ITUNESOWNER
        file.AdditionalFields["rtng"] = track.IsExplicit ? "1" : "0";                   // ITUNESADVISORY 
        file.AdditionalFields["purd"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");   // ITUNESPURCHASEDATE

        // Picture field
        if (saveArtwork && track.ArtworkUrl is not null)
        {
            Stream pictureStream = await pictureHandler.GetStreamAsync(track.ArtworkUrl, cancellationToken);
            byte[] pictureData = pictureHandler.Resize(pictureStream);

            PictureInfo picture = PictureInfo.fromBinaryData(pictureData);
            file.EmbeddedPictures.Add(picture);
        }

        await file.SaveAsync();
        logger?.LogInformation("[ITunesPlugin-WriteAsync] Wrote track metadata");
    }
}
