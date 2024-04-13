namespace Musify.Models;

public class Track(
    string title,
    string artist,
    TimeSpan duration,
    string artwork)
{
    public string Title { get; } = title;

    public string Artist { get; } = artist;

    public TimeSpan Duration { get; } = duration;

    public string Artwork { get; } = artwork;
}