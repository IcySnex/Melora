# SearchResult
Represents a search result\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
```cs
public class SearchResult
```


## Constructors
Creates a new SearchResult\.
```cs
public SearchResult(
  string title, 
  string artists, 
  TimeSpan duration, 
  string imageUrl, 
  string id, 
  Dictionary<string, object> items)
```
| Parameter | Summary |
| --------- | ------- |
| `string` title | The title of the search result. |
| `string` artists | The artists of the search result. |
| `TimeSpan` duration | The duration of the search result. |
| `string` imageUrl | The url to the image of the search result. |
| `string` id | The id of the search result. |
| `Dictionary<string, object>` items | Additional information items for the track. |



## Methods

### BufferAsync
Converts and buffers an async enumerable to a list of search results\.
```cs
public static Task<IEnumerable<SearchResult>> BufferAsync(
  IAsyncEnumerable<T> source, 
  int limit, 
  Func<T, int, SearchResult> createSearchResult, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `IAsyncEnumerable<T>` source | The source async enumerable |
| `int` limit | The limit of search results to buffer. |
| `Func<T, int, SearchResult>` createSearchResult | The function to create a new search result. |
| `CancellationToken` cancellationToken | The token to cancel this action. |



## Properties

### Title
The title of the search result\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Artists
The artists of the search result\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Duration
The duration of the search result\.
- **Type:** [System.TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
- **Is Read Only:** `True`

### ImageUrl
The url to the image of the search result\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Id
The id of the search result\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`
