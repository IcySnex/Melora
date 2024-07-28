using Musify.Plugins.Models;

namespace Musify.Plugins.Exceptions;

/// <summary>
/// Represents errors that occurr when an requested item of a search result is invalid in.
/// </summary>
/// <remarks>
/// Creates a new instance of SearchResultInvalidItemException
/// </remarks>
/// <param name="requestedItem">The name of the rquested search result item.</param>
/// <param name="searchResult">The search result from which the item was requested.</param>
/// <param name="innerException">The inner exception.</param>
public class SearchResultInvalidItemException(
    string requestedItem,
    SearchResult searchResult,
    Exception? innerException = null) : Exception("Item of search result is invalid.", innerException)
{
    /// <summary>
    /// The name of the rquested search result item.
    /// </summary>
    public string RequestedItem { get; } = requestedItem;

    /// <summary>
    /// The search result from which the item was requested.
    /// </summary>
    public SearchResult SearchResult { get; } = searchResult;
}