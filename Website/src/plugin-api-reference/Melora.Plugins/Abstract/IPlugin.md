# IPlugin
Represents a plugin\.
- **Type:** Interface
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
```cs
public interface IPlugin
```




## Properties

### Kind
The kind of the plugin\.
- **Type:** [Melora.Plugins.Enums.PluginKind](/Melora/plugin-api-reference/Melora.Plugins/Enums/PluginKind.html)
- **Is Read Only:** `True`

### Name
The name the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### IconPathData
The svg path data for the plugin icon\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Config
The config for the plugin\.
- **Type:** [Melora.Plugins.Abstract.IPluginConfig](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IPluginConfig.html)
- **Is Read Only:** `True`
