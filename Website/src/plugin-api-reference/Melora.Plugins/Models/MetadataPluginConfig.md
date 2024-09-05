# MetadataPluginConfig
Describes a configuration for a metadata plugin
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging), [IPluginConfig](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IPluginConfig.html)
```cs
public class MetadataPluginConfig : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, IPluginConfig
```


## Constructors
Creates a new MetadataPluginConfig
```cs
public MetadataPluginConfig(
  IOption[] options)
```
| Parameter | Summary |
| --------- | ------- |
| `IOption[]` options | Additional config options for the plugin. |

Creates a new MetadataPluginConfig
```cs
public MetadataPluginConfig(
  IOption[] defaultOptions, 
  MetadataPluginConfig initialConfig)
```
| Parameter | Summary |
| --------- | ------- |
| `IOption[]` defaultOptions | Default additional config options for the plugin. |
| `MetadataPluginConfig` initialConfig | The config used for initializing if exists. |



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
