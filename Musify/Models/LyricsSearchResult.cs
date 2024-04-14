namespace Musify.Models;

public class LyricsSearchResult(
    string title,
    string artists,
    string type,
    string url,
    //string artwork,
    string artworkThumbnail)
{
    public string Title { get; set; } = title;

    public string Artists { get; set; } = artists;

    public string Type { get; set; } = type;

    public string Url { get; set; } = url;

    //public string Artwork { get; set; } = artwork;

    public string ArtworkThumbnail { get; set; } = artworkThumbnail;
}