using CommunityToolkit.Mvvm.ComponentModel;

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
/// <param name="trackNumber">The track number of the downloadable track.</param>
/// <param name="totalTracks">The total tracks of the downloadable tracks album.</param>
/// <param name="copyright">The copyright of the downloadable tracks.</param>
/// <param name="comment">An optional comment from the plugin.</param>
/// <param name="url">The url of the downloadable track.</param>
/// <param name="id">The Id of the downloadable track.</param>
/// <param name="pluginHash">The hash code for the plugin responsible for this downloadable track.</param>
/// <remarks>
/// Creates a new DownloadableTrack.
/// </remarks>
public partial class DownloadableTrack(
    string title,
    string artists,
    TimeSpan duration,
    string? artworkUrl,
    bool isExplicit,
    DateTime releasedAt,
    string? album,
    string? genre,
    string? lyrics,
    int trackNumber,
    int totalTracks,
    string copyright,
    string? comment,
    string url,
    string id,
    int pluginHash) : ObservableObject
{
    /// <summary>
    /// The title of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string title = title;

    /// <summary>
    /// The artists of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string artists = artists;

    /// <summary>
    /// The duration of the downloadable track.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The url to the artwork of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string? artworkUrl = artworkUrl;

    /// <summary>
    /// Whether the downloadable track is explicit or not.
    /// </summary>
    [ObservableProperty]
    bool isExplicit = isExplicit;

    /// <summary>
    /// The date and time when the downloadable track was released.
    /// </summary>
    [ObservableProperty]
    DateTime releasedAt = releasedAt;

    /// <summary>
    /// The album of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string? album = album;

    /// <summary>
    /// The genre of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string? genre = genre;

    /// <summary>
    /// The lyrics of the downloadable track.
    /// </summary>
    [ObservableProperty]
    string? lyrics = lyrics;

    /// <summary>
    /// The track number of the downloadable track.
    /// </summary>
    [ObservableProperty]
    int trackNumber = trackNumber;

    /// <summary>
    /// The total tracks of the downloadable track's album.
    /// </summary>
    [ObservableProperty]
    int totalTracks = totalTracks;
    
    /// <summary>
    /// The copyright of the downloadable track's album.
    /// </summary>
    [ObservableProperty]
    string copyright = copyright;

    /// <summary>
    /// An optional comment from the plugin.
    /// </summary>
    public string? Comment { get; } = comment;
    
    /// <summary>
    /// The url of the downloadable track.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// The Id of the downloadable track.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The hash code for the plugin responsible for this downloadable track.
    /// </summary>
    public int PluginHash { get; } = pluginHash;
}