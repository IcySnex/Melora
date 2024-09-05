# IPluginConfig
Describes a configuration for a plugin
- **Type:** Interface
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
```cs
public interface IPluginConfig
```


## Methods

### Reset
Resets the config to the plugins default\.
```cs
public abstract Void Reset()
```


## Properties

### Options
Additional config options for the plugin\.
- **Type:** [Melora.Plugins.Abstract.IOption[]](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IOption.html)
- **Is Read Only:** `True`
