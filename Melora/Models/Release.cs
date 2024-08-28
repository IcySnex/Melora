using Melora.Enums;
using Melora.Services;
using Newtonsoft.Json;

namespace Melora.Models;

public class Release(
    [JsonProperty("tag_name")] string tag,
    [JsonProperty("name")] string name,
    [JsonProperty("published_at")] DateTime publishedAt,
    [JsonProperty("author")] ReleaseUser author,
    [JsonProperty("assets")] ReleaseAsset[] assets,
    [JsonProperty("body")] string body)
{
    public string Name { get; } = name;

    public DateTime PublishedAt { get; } = publishedAt;

    public ReleaseUser Author { get; } = author;

    public string Body { get; } = body;

    public ReleaseAsset? Binary { get; } = assets.FirstOrDefault(asset => asset.ContentType == "application/zip" && asset.Name == $"Melora.win10.{UpdateManager.Architecture}.zip");

    public Version Version { get; } = tag.Length < 6 ? new(1, 0, 0) : Version.Parse(tag.AsSpan(1, 5));

    public UpdateChannel Channel { get; } = tag.Length < 7 ? UpdateChannel.Stable : tag[7..] switch
    {
        "stable" => UpdateChannel.Stable,
        "beta" => UpdateChannel.Beta,
        "alpha" => UpdateChannel.Alpha,
        _ => UpdateChannel.Stable
    };
}


public class ReleaseUser(
    [JsonProperty("login")] string name)
{
    public string Name { get; } = name;
}

public class ReleaseAsset(
    [JsonProperty("name")] string name,
    [JsonProperty("content_type")] string contentType,
    [JsonProperty("download_count")] int totalDownloads,
    [JsonProperty("size")] int sizeInBytes,
    [JsonProperty("updated_at")] DateTime updatedAt,
    [JsonProperty("browser_download_url")] string downloadUrl)
{
    public string Name { get; } = name;

    public string ContentType { get; } = contentType;

    public int TotalDownloads { get; } = totalDownloads;

    public double SizeInMb { get; } = sizeInBytes / 1000000.0;

    public DateTime UpdatedAt { get; } = updatedAt;

    public string DownloadUrl { get; } = downloadUrl;
}