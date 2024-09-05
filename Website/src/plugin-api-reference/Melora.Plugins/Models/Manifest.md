# Manifest
Contains information for a plugin\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
```cs
public class Manifest
```


## Constructors
Creates a new Manifest\.
```cs
public Manifest(
  string name, 
  string description, 
  string author, 
  Version apiVersion, 
  DateTime lastUpdatedAt, 
  string sourceUrl, 
  PluginKind[] pluginKinds, 
  string entryPoint, 
  String[] dependencies)
```
| Parameter | Summary |
| --------- | ------- |
| `string` name | The name of the plugin. |
| `string` description | The description of the plugin. |
| `string` author | The author of the plugin. |
| `Version` apiVersion | The version of the plugins API used. |
| `DateTime` lastUpdatedAt | The date and time when the plugin was last updated.. |
| `string` sourceUrl | The url to the source of the plugin (e.g. GitHub project, Website...). |
| `PluginKind[]` pluginKinds | The kinds of plugins contained. |
| `string` entryPoint | The path to the entry point of the plugin. |
| `String[]` dependencies | The paths to the dependencies of the plugin. |



## Methods

### FromPluginArchivetAsync
Gets the manifest from a plugin archive\.
```cs
public static Task<Manifest> FromPluginArchivetAsync(
  ZipArchive pluginArchive, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `ZipArchive` pluginArchive | The plugin archive to load the manifest from. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |



## Properties

### Name
The name of the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Description
The description of the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Author
The author of the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### ApiVersion
The version of the plugins API used\.
- **Type:** [System.Version](https://learn.microsoft.com/dotnet/api/system.version)
- **Is Read Only:** `True`

### LastUpdatedAt
The date and time when the plugin was last updated\.
- **Type:** [System.DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)
- **Is Read Only:** `True`

### SourceUrl
The url to the source of the plugin \(e\.g\. GitHub project, Website\.\.\.\)\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### PluginKinds
The kinds of plugins contained\.
- **Type:** [Melora.Plugins.Enums.PluginKind[]](/Melora/plugin-api-reference/Melora.Plugins/Enums/PluginKind.html)
- **Is Read Only:** `True`

### EntryPoint
The path to the entry point of the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Dependencies
The paths to the dependencies of the plugin\.
- **Type:** [System.String[]](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`
