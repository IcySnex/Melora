using Musify.Json;
using System.Text.Json.Serialization;

namespace Musify.Models;

public class LyricsRequestResult
{
    [JsonPropertyName("meta")]
    public LyricsRequestMeta Meta { get; set; } = default!;

    [JsonPropertyName("response")]
    public LyricsRequestResponse Response { get; set; } = default!;
}

public class LyricsRequestMeta
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; } = default!;

    [JsonPropertyName("message")]
    public string? Message { get; set; } = null;
}

public class LyricsRequestResponse
{
    [JsonPropertyName("hits")]
    public LyricsHit[] Hits { get; set; } = [];
}

public class LyricsHit
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("result")]
    public LyricsTrack Track { get; set; } = default!;
}

public class LyricsTrack
{
    [JsonPropertyName("annotation_count")]
    public int AnnotationCount { get; set; } = default!;

    [JsonPropertyName("api_path")]
    public string ApiPath { get; set; } = default!;

    [JsonPropertyName("artist_names")]
    public string ArtistNames { get; set; } = default!;

    [JsonPropertyName("full_title")]
    public string FullTitle { get; set; } = default!;

    [JsonPropertyName("header_image_thumbnail_url")]
    public string HeaderImageThumbnailUrl { get; set; } = default!;

    [JsonPropertyName("header_image_url")]
    public string HeaderImageUrl { get; set; } = default!;

    [JsonPropertyName("id")]
    public int Id { get; set; } = default!;

    [JsonPropertyName("lyrics_owner_id")]
    public int LyricsOwnerId { get; set; } = default!;

    [JsonPropertyName("lyrics_state")]
    public string LyricsState { get; set; } = default!;

    [JsonPropertyName("path")]
    public string Path { get; set; } = default!;

    [JsonPropertyName("pyongs_count")]
    public int? PyongsCount { get; set; } = null;

    [JsonPropertyName("relationships_index_url")]
    public string RelationshipsIndexUrl { get; set; } = default!;

    [JsonPropertyName("release_date_components")]
    [JsonConverter(typeof(DateComponentsConverter))]
    public DateTime ReleaseDate { get; set; } = DateTime.MinValue;

    [JsonPropertyName("song_art_image_thumbnail_url")]
    public string ArtworkThumbnailUrl { get; set; } = default!;

    [JsonPropertyName("song_art_image_url")]
    public string ArtworklUrl { get; set; } = default!;

    [JsonPropertyName("stats")]
    public LyricTrackStats Stats { get; set; } = default!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("title_with_featured")]
    public string TitleWithFeatured { get; set; } = default!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    [JsonPropertyName("featured_artists")]
    public LyricsArtist[] FeaturedArtists { get; set; } = [];

    [JsonPropertyName("primary_artist")]
    public LyricsArtist PrimaryArtist { get; set; } = default!;
}

public class LyricTrackStats
{
    [JsonPropertyName("unreviewed_annotations")]
    public int UnreviewedAnnotations { get; set; } = default!;

    [JsonPropertyName("concurrents")]
    public int? Concurrents { get; set; } = null;

    [JsonPropertyName("hot")]
    public bool IsHot { get; set; } = default!;

    [JsonPropertyName("pageviews")]
    public int PageViews { get; set; } = default!;
}

public class LyricsArtist
{
    [JsonPropertyName("api_path")]
    public string ApiPath { get; set; } = default!;

    [JsonPropertyName("header_image_url")]
    public string HeaderImageUrl { get; set; } = default!;

    [JsonPropertyName("id")]
    public int Id { get; set; } = default!;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = default!;

    [JsonPropertyName("is_meme_verified")]
    public bool IsMemeVerified { get; set; } = default!;

    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    [JsonPropertyName("iq")]
    public int Iq { get; set; } = default!;
}