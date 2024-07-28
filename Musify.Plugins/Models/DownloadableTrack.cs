using Musify.Plugins.Abstract;

namespace Musify.Plugins.Models;

/// <summary>
/// Represents a track that can be downloaded.
/// </summary>
/// <param name="title">The title of the downloadable track.</param>
/// <param name="artists">The artists of the downloadable track.</param>
/// <param name="duration">The duration of the downloadable track.</param>
/// <param name="artworkUrl">The url to the artwork of the downloadable track.</param>
/// <param name="isExplicit">Weither the downloadable track is explicit or not.</param>
/// <param name="releasedAt">The date and time when the downloadable track was released.</param>
/// <param name="album">The album of the downloadable track.</param>
/// <param name="genre">The genre of the downloadable track.</param>
/// <param name="lyrics">The lyrics of the downloadable track.</param>
/// <param name="url">The url of the downloadable track.</param>
/// <param name="trackNumber">The track number of the downloadable track.</param>
/// <param name="totalTracks">The total tracks of the downloadable tracks album.</param>
/// <param name="id">The Id of the downloadable track.</param>
/// <param name="plugin">The plugin responsible for this downloadable track.</param>
/// <remarks>
/// Creates a new DownloadableTrack.
/// </remarks>
public class DownloadableTrack(
    string title,
    string artists,
    TimeSpan duration,
    string? artworkUrl,
    bool isExplicit,
    DateTime releasedAt,
    string? album,
    string? genre,
    string? lyrics,
    string url,
    int trackNumber,
    int totalTracks,
    string id,
    PlatformSupportPlugin plugin)
{
    /// <summary>
    /// The title of the downloadable track.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// The artists of the downloadable track.
    /// </summary>
    public string Artists { get; } = artists;

    /// <summary>
    /// The duration of the downloadable track.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The url to the artwork of the downloadable track.
    /// </summary>
    public string? ArtworkUrl { get; } = artworkUrl;

    /// <summary>
    /// Weither the downloadable track is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;
    
    /// <summary>
    /// The date and time when the downloadable track was released.
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;
    
    /// <summary>
    /// The album of the downloadable track.
    /// </summary>
    public string? Album { get; } = album;
    
    /// <summary>
    /// The genre of the downloadable track.
    /// </summary>
    public string? Genre { get; } = genre;

    /// <summary>
    /// The lyrics of the downloadable track.
    /// </summary>
    public string? Lyrics { get; } = lyrics;

    /// <summary>
    /// The track number of the downloadable track.
    /// </summary>
    public int TrackNumber { get; } = trackNumber;

    /// <summary>
    /// The total tracks of the downloadable tracks album.
    /// </summary>
    public int TotalTracks { get; } = totalTracks;

    /// <summary>
    /// The url of the downloadable track.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// The Id of the downloadable track.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The plugin responsible for this downloadable track.
    /// </summary>
    public PlatformSupportPlugin Plugin { get; } = plugin;
}