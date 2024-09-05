# PlatformSupportPluginConfig
Describes a configuration for a platfrom support plugin
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging), [IPluginConfig](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IPluginConfig.html)
```cs
public class PlatformSupportPluginConfig : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, IPluginConfig
```


## Constructors
Creates a new PlatformSupportPluginConfig
```cs
public PlatformSupportPluginConfig(
  IOption[] options, 
  Quality quality, 
  Format format, 
  Nullable<int> searchResultsLimit, 
  Sorting searchResultsSorting, 
  bool searchResultsSortDescending)
```
| Parameter | Summary |
| --------- | ------- |
| `IOption[]` options | Additional config options for the plugin. |
| `Quality` quality | The quality in which tracks get downloaded. |
| `Format` format | The format in which tracks get downloaded. |
| `Nullable<int>` searchResultsLimit | The limit of search results to fetch. |
| `Sorting` searchResultsSorting | The sorting of search results. |
| `bool` searchResultsSortDescending | The view options of the search page for the platform. |

Creates a new PlatformSupportPluginConfig
```cs
public PlatformSupportPluginConfig(
  IOption[] defaultOptions, 
  Quality defaultQuality, 
  Format defaultFormat, 
  Nullable<int> defaultSearchResultsLimit, 
  Sorting defaultSearchResultsSorting, 
  bool defaultSearchResultsSortDescending, 
  PlatformSupportPluginConfig initialConfig)
```
| Parameter | Summary |
| --------- | ------- |
| `IOption[]` defaultOptions | Default additional config options for the plugin. |
| `Quality` defaultQuality | The default quality in which tracks get downloaded. |
| `Format` defaultFormat | The default format in which tracks get downloaded. |
| `Nullable<int>` defaultSearchResultsLimit | The default limit of search results to fetch. |
| `Sorting` defaultSearchResultsSorting | The default sorting of search results. |
| `bool` defaultSearchResultsSortDescending | The default view options of the search page for the platform. |
| `PlatformSupportPluginConfig` initialConfig | The config used for initializing if exists. |



## Methods

### Reset
Resets the config to the plugins default\.
```cs
public Void Reset()
```


## Properties

### Options
Additional config options for the plugin\.
- **Type:** [Melora.Plugins.Abstract.IOption[]](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IOption.html)
- **Is Read Only:** `True`

### Quality
The quality in which tracks get downloaded\.
- **Type:** [Melora.Plugins.Enums.Quality](/Melora/plugin-api-reference/Melora.Plugins/Enums/Quality.html)
- **Is Read Only:** `False`

### Format
The format in which tracks get downloaded\.
- **Type:** [Melora.Plugins.Enums.Format](/Melora/plugin-api-reference/Melora.Plugins/Enums/Format.html)
- **Is Read Only:** `False`

### SearchResultsLimit
The limit of search results to fetch\.
- **Type:** [System.Nullable<System.Int32>](https://learn.microsoft.com/dotnet/api/system.nullable<int>)
- **Is Read Only:** `False`

### SearchResultsSorting
The sorting of search results\.
- **Type:** [Melora.Plugins.Enums.Sorting](/Melora/plugin-api-reference/Melora.Plugins/Enums/Sorting.html)
- **Is Read Only:** `False`

### SearchResultsSortDescending
Whether search results are sorted descending or not\.
- **Type:** [System.Boolean](https://learn.microsoft.com/dotnet/api/system.bool)
- **Is Read Only:** `False`
