#pragma warning disable IDE0052 // Remove unread private members <= Thanks to dumb ahh System.Text.Json - doeesn't even support JsonProprty in constructor: Akschoully each parameter in the constr must bind... ☝️🤓 - SHUT UP BRO

using Melora.Enums;
using Melora.Services;
using System.Text.Json.Serialization;

namespace Melora.Models;

public class Release(
    string tag,
    string name,
    DateTime publishedAt,
    ReleaseUser author,
    ReleaseAsset[] assets,
    string body)
{
    [JsonInclude]
    [JsonPropertyName("tag_name")]
    private readonly string tag = tag;

    [JsonInclude]
    [JsonPropertyName("assets")]
    private readonly ReleaseAsset[] assets = assets;


    [JsonPropertyName("name")]
    public string Name { get; } = name;

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; } = publishedAt;

    [JsonPropertyName("author")]
    public ReleaseUser Author { get; } = author;

    [JsonPropertyName("body")]
    public string Body { get; } = body;


    public ReleaseAsset? Binary { get; } = assets.FirstOrDefault(asset => asset.ContentType == "application/zip" && asset.Name == $"Melora.win10.{UpdateManager.Architecture}.zip");

    public Version Version { get; } = tag.Length < 6 ? new Version(1, 0, 0) : Version.Parse(tag.AsSpan(1, 5).ToString());

    public UpdateChannel Channel { get; } = tag.Length < 7 ? UpdateChannel.Stable : tag[7..] switch
    {
        "stable" => UpdateChannel.Stable,
        "beta" => UpdateChannel.Beta,
        "alpha" => UpdateChannel.Alpha,
        _ => UpdateChannel.Stable
    };
}


public class ReleaseUser(
    string name)
{
    [JsonPropertyName("login")]
    public string Name { get; } = name;
}

public class ReleaseAsset(
    string name,
    string contentType,
    int totalDownloads,
    int sizeInBytes,
    DateTime updatedAt,
    string downloadUrl)
{
    [JsonInclude]
    [JsonPropertyName("size")]
    private readonly int sizeInBytes = sizeInBytes;


    [JsonPropertyName("name")]
    public string Name { get; } = name;

    [JsonPropertyName("content_type")]
    public string ContentType { get; } = contentType;

    [JsonPropertyName("download_count")]
    public int TotalDownloads { get; } = totalDownloads;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; } = updatedAt;

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; } = downloadUrl;

    public double SizeInMb { get; } = sizeInBytes / 1000000.0;
}
