using Musify.Plugins.Exceptions;

namespace Musify.Plugins.Models;

/// <summary>
/// Represents a search result.
/// </summary>
/// <remarks>
/// Creates a new SearchResult.
/// </remarks>
/// <param name="title">The title of the search result.</param>
/// <param name="artists">The artists of the search result.</param>
/// <param name="duration">The duration of the search result.</param>
/// <param name="imageUrl">The url to the image of the search result.</param>
/// <param name="id">The id of the search result.</param>
/// <param name="items">Additional information items for the track.</param>
public class SearchResult(
    string title,
    string artists,
    TimeSpan duration,
    string? imageUrl,
    string id,
    Dictionary<string, object?> items)
{
    /// <summary>
    /// Converts and buffers an async enumerable to a list of search results.
    /// </summary>
    /// <typeparam name="T">The type to convert from.</typeparam>
    /// <param name="source">The source async enumerable</param>
    /// <param name="limit">The limit of search results to buffer.</param>
    /// <param name="createSearchResult">The function to create a new search result.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>A list of search results.</returns>
    public static async Task<IEnumerable<SearchResult>> BufferAsync<T>(
        IAsyncEnumerable<T> source,
        int limit,
        Func<T, int, SearchResult?> createSearchResult,
        CancellationToken cancellationToken)
    {
        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);

        List<SearchResult> results = [];
        while (limit > results.Count && await enumerator.MoveNextAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (createSearchResult.Invoke(enumerator.Current, results.Count) is SearchResult result)
                results.Add(result);
        }
        return results;
    }


    /// <summary>
    /// The title of the search result.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// The artists of the search result.
    /// </summary>
    public string Artists { get; } = artists;

    /// <summary>
    /// The duration of the search result.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The url to the image of the search result.
    /// </summary>
    public string? ImageUrl { get; } = imageUrl;

    /// <summary>
    /// The id of the search result.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Additional information items for the track.
    /// </summary>
    public Dictionary<string, object?> Items = items;
}


/// <summary>
/// Contains extension methods for search results.
/// </summary>
public static class SearchResultExtensions
{
    /// <summary>
    /// Gets an item of a search result with the name.
    /// </summary>
    /// <typeparam name="T">The requsted type of the item.</typeparam>
    /// <param name="searchResult">The search result to get the item from.</param>
    /// <param name="name">The name of the requested item.</param>
    /// <returns>The requested item.</returns>
    /// <exception cref="SearchResultInvalidItemException">Occurrs when the item could not be found or does not represent the requested type.</exception>
    public static T GetItem<T>(
        this SearchResult searchResult,
        string name)
    {
        if (!searchResult.Items.TryGetValue(name, out object? item))
            throw new SearchResultInvalidItemException(name, searchResult, new("Name was not found."));

        if (item is not T && item is not null)
            throw new SearchResultInvalidItemException(name, searchResult, new("Item does not represent requested type."));

        return (T)item!;
    }
}