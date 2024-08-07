#pragma warning disable IDE0290 // Use primary constructor

using ATL;
using Microsoft.Extensions.Logging;
using Musify.Plugins.Abstract;
using Musify.Plugins.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Musify.Metadata.Default;

public class DefaultPlugin : MetadataPlugin
{
    public DefaultPlugin(
        MetadataPluginConfig? config,
        ILogger<IPlugin>? logger) : base(
            "Default",
            "M272.5 512a99.3 99.3 0 0 1-30.9-4.9c-22.8-7.4-39.8-24.4-73.6-58.2L43.3 324.3c-14.4-14.4-22.4-22.4-28.5-32.5-5.5-9-9.5-18.7-12-28.9C0 251.4 0 240.1 0 219.7v-71.9C0 100 0 76 10.9 54.6c9.7-19 24.8-34.1 43.7-43.7C76.1 0 100 0 147.8 0h71.9c20.4 0 31.6 0 43.1 2.8 10.2 2.5 20 6.5 28.9 12 10.1 6.2 18 14.1 32.4 28.5L448.8 168c33.8 33.8 50.7 50.7 58.2 73.6 6.6 20.2 6.6 41.6 0 61.9-7.4 22.9-24.4 39.8-58.2 73.6L377 448.9c-33.8 33.8-50.7 50.7-73.6 58.2a99.3 99.3 0 0 1-30.9 4.9zM147.8 41.1c-41.2 0-61.9 0-74.5 6.4-11.2 5.7-20.1 14.6-25.8 25.8-6.4 12.6-6.4 33.3-6.4 74.5v71.9c0 17.8 0 26.8 1.6 33.5a56.86 56.86 0 0 0 7.1 17.1c3.6 5.9 10 12.3 22.5 24.8L197 419.9c29.1 29.1 43.8 43.8 57.2 48.1 11.9 3.9 24.5 3.9 36.5 0 13.5-4.4 28.1-19 57.2-48.1l71.9-71.9c29.1-29.1 43.8-43.8 48.1-57.2 3.9-11.9 3.9-24.5 0-36.5-4.4-13.5-19-28.1-48.1-57.2L295.2 72.3c-12.6-12.6-18.9-18.9-24.9-22.6-5.3-3.2-11-5.6-17.1-7.1-6.8-1.6-15.7-1.6-33.5-1.6h-71.9zm1.1 141.6c-18.6 0-33.8-15.2-33.8-33.8s15.2-33.8 33.8-33.8 33.8 15.2 33.8 33.8c0 18.7-15.1 33.8-33.8 33.8z",
            new(
                defaultItems:
                [
                    new("Artists Seperator", "The string used to seperate multiple artists.", ", "),
                    new("Save Artwork", "Whether to save the artwork.", true),
                    new("Artwork Resolution", "The resolution (nXn) the artwork gets resized to before saving.", 256L)
                ],
                initialConfig: config),
            logger)
    { }

    public DefaultPlugin(
        MetadataPluginConfig? config) : this(config, null)
    { }

    public DefaultPlugin(
        ILogger<IPlugin>? logger) : this(null, logger)
    { }

    public DefaultPlugin() : this(null, null)
    { }


    [SupportedOSPlatform("windows")]
    public override async Task WriteAsync(
        string filePath,
        DownloadableTrack track,
        CancellationToken cancellationToken = default)
    {
        string artistsSeperator = Config.GetItem<string>("Artists Seperator");
        bool saveArtwork = Config.GetItem<bool>("Save Artwork");
        int artworkResolution = (int)Config.GetItem<long>("Artwork Resolution");

        logger?.LogInformation("[DefaultPlugin-WriteAsync] Starting to write track metadata...");
        Track file = new(filePath);

        // Default fields
        file.Title = track.Title;
        file.Artist = track.Artists.Replace(", ", artistsSeperator);
        file.Date = track.ReleasedAt;
        file.Album = track.Album ?? string.Empty;
        file.AlbumArtist = track.Artists.Split(", ")[0];
        file.Genre = track.Genre ?? string.Empty;
        file.Lyrics.UnsynchronizedLyrics = track.Lyrics ?? string.Empty;
        file.TrackNumber = track.TrackNumber;
        file.TrackTotal = track.TotalTracks;
        file.Copyright = track.Copyright;
        file.Comment = track.Comment ?? string.Empty;
        file.AudioSourceUrl = track.Url;

        // Additional fields
        file.AdditionalFields["EXPLICIT"] = track.IsExplicit ? "1" : "0";

        // Picture field
        if (saveArtwork && track.ArtworkUrl is not null)
        {
            Stream pictureStream = await GetPictureStreamAsync(track.ArtworkUrl, cancellationToken);
            byte[] pictureData = ResizePicture(pictureStream, artworkResolution);

            PictureInfo picture = PictureInfo.fromBinaryData(pictureData);
            file.EmbeddedPictures.Add(picture);
        }

        await file.SaveAsync();
        logger?.LogInformation("[DefaultPlugin-WriteAsync] Wrote track metadata");
    }


    readonly HttpClient client = new();

    async Task<Stream> GetPictureStreamAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[DefaultPlugin-WriteAsync] Getting picture stream...");

        return url.StartsWith("file:///")
            ? File.OpenRead(url[8..])
            : await client.GetStreamAsync(url, cancellationToken);
    }

    [SupportedOSPlatform("windows")]
    byte[] ResizePicture(Stream stream, int resolution)
    {
        logger?.LogInformation("Resizing picture to resolution: {resolution}...", resolution);
        using Image image = Image.FromStream(stream);
        using Bitmap resized = new(resolution, resolution);
        using Graphics graphics = Graphics.FromImage(resized);
        using MemoryStream result = new();

        double scaleX = (double)resolution / image.Width;
        double scaleY = (double)resolution / image.Height;
        double scale = Math.Max(scaleX, scaleY);

        int newWidth = (int)(image.Width * scale);
        int newHeight = (int)(image.Height * scale);
        int xOffset = (resolution - newWidth) / 2;
        int yOffset = (resolution - newHeight) / 2;

        graphics.DrawImage(image, xOffset, yOffset, newWidth, newHeight);
        resized.Save(result, ImageFormat.Jpeg);

        return result.ToArray();
    }
}