namespace Musify.Plugins.Enums;

/// <summary>
/// Describes in which order items are sorted.
/// </summary>
public enum Sorting
{
    /// <summary>
    /// Default sorting, provided from platform.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by title.
    /// </summary>
    Title,

    /// <summary>
    /// Sort by artist name.
    /// </summary>
    Artist,

    /// <summary>
    /// Sort by duration.
    /// </summary>
    Duration
}