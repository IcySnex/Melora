# PlatformSupportPlugin
Represents a plugin for additional platform support\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
- **Implements:**  [IPlugin](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IPlugin.html)
```cs
public class PlatformSupportPlugin : IPlugin
```




## Methods

### SearchAsync
Searches for a query on the platform\.
```cs
public abstract Task<IEnumerable<SearchResult>> SearchAsync(
  string query, 
  IProgress<string> progress, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `string` query | The query to search for. |
| `IProgress<string>` progress | The progress to report to. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |

### PrepareDownloadsAsync
Prepares search results for downloads\.
```cs
public abstract Task<IEnumerable<DownloadableTrack>> PrepareDownloadsAsync(
  IEnumerable<SearchResult> searchResults, 
  IProgress<string> progress, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `IEnumerable<SearchResult>` searchResults | The search results to prepare for downloads. |
| `IProgress<string>` progress | The progress to report to. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |

### GetStreamAsync
Gets the stream of a downloadable track\.
```cs
public abstract Task<Stream> GetStreamAsync(
  DownloadableTrack track, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `DownloadableTrack` track | The downloadable track to get the stream from. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |



## Properties

### Kind
The kind of the plugin\.
- **Type:** [Melora.Plugins.Enums.PluginKind](/Melora/plugin-api-reference/Melora.Plugins/Enums/PluginKind.html)
- **Is Read Only:** `True`

### Name
The name of the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### IconPathData
The svg path data for the plugin icon\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Config
The config for the plugin\.
- **Type:** [Melora.Plugins.Models.PlatformSupportPluginConfig](/Melora/plugin-api-reference/Melora.Plugins/Models/PlatformSupportPluginConfig.html)
- **Is Read Only:** `True`
