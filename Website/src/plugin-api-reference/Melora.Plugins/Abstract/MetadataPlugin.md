# MetadataPlugin
Represents a plugin which writes track metadata after downloading\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
- **Implements:**  [IPlugin](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IPlugin.html)
```cs
public class MetadataPlugin : IPlugin
```




## Methods

### WriteAsync
Writes the metadate from the downloadable track to the file\.
```cs
public abstract Task WriteAsync(
  string filePath, 
  DownloadableTrack track, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `string` filePath | The path to the audio file. |
| `DownloadableTrack` track | The downloadable track containing the metadata. |
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
- **Type:** [Melora.Plugins.Models.MetadataPluginConfig](/Melora/plugin-api-reference/Melora.Plugins/Models/MetadataPluginConfig.html)
- **Is Read Only:** `True`
