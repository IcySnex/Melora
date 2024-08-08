using Microsoft.Extensions.Logging;
using Musify.Plugins.Abstract;
using Musify.Plugins.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Musify.Metadata.Default.Internal;

public class PictureHandler
{
    readonly MetadataPluginConfig config;
    readonly ILogger<IPlugin>? logger;

    HttpClient httpClient = default!;

    public PictureHandler(
        MetadataPluginConfig config,
        ILogger<IPlugin>? logger)
    {
        this.config = config;
        this.logger = logger;

        AuthenticatsHttpClient();

        logger?.LogInformation("[PictureHandler-.ctor] PictureHandler has been initialized.");
    }


    public void AuthenticatsHttpClient()
    {
        httpClient = new();

        logger?.LogInformation("[PictureHandler-AuthenticatsHttpClient] HTTP client has been authenticated.");
    }


    public async Task<Stream> GetStreamAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[PictureHandler-GetStreamAsync] Getting picture stream...");

        return url.StartsWith("file:///")
            ? File.OpenRead(url[8..])
            : await httpClient.GetStreamAsync(url, cancellationToken);
    }


    [SupportedOSPlatform("windows")]
    public byte[] Resize(
        Stream stream)
    {
        int resolution = (int)config.GetItem<long>("Artwork Resolution");

        logger?.LogInformation("[PictureHandler-Resize] Resizing picture to resolution: {resolution}...", resolution);

        using Bitmap resized = new(resolution, resolution);
        using MemoryStream result = new();
        using Image image = Image.FromStream(stream);
        using Graphics graphics = Graphics.FromImage(resized);

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