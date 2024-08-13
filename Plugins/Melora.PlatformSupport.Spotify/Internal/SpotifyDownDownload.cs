using Newtonsoft.Json;

namespace Melora.PlatformSupport.Spotify.Internal;

internal class SpotifyDownDownload
{
    [JsonProperty("success")]
    public bool IsSuccessful { get; set; } = default!;

    [JsonProperty("message")]
    public string? ErrorMessage { get; set; } = null;

    [JsonProperty("metaData")]
    public SpotifyDownDownloadMetaData MetaData { get; set; } = default!;

    [JsonProperty("link")]
    public string StreamUrl { get; set; } = default!;
}

internal class SpotifyDownDownloadMetaData
{
    [JsonProperty("success")]
    public bool IsSuccessful { get; set; } = default!;

    [JsonProperty("cache")]
    public bool FromCache { get; set; } = default!;

    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("artists")]
    public string Artists { get; set; } = default!;

    [JsonProperty("title")]
    public string Title { get; set; } = default!;

    [JsonProperty("album")]
    public string Album { get; set; } = default!;

    [JsonProperty("cover")]
    public string CoverUrl { get; set; } = default!;

    [JsonProperty("isrc")]
    public string Isrc { get; set; } = default!;

    [JsonProperty("releaseDate")]
    public string ReleasedAt { get; set; } = default!;
}