using Musify.Enums;

namespace Musify.Models;

public class Track(
    string title,
    string artist,
    TimeSpan duration,
    string? artwork,
    string? album,
    Source source,
    string url)
{
    public string Title { get; } = title;

    public string Artist { get; } = artist;

    public TimeSpan Duration { get; } = duration;

    public string? Artwork { get; } = artwork;

    public string? Album { get; } = album;

    public Source Source { get; } = source;

    public string Url { get; } = url;
}