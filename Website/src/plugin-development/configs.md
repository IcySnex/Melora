---
title: Configs
icon: icon-park-outline:setting-config
order: 4
---

When building a plugin, you may need to store settings that users can customize. Melora makes this easy by integrating your plugin's configs into the main Melora configuration.


## What Is A Config?
When initializing your plugin class, you need to provide a corresponding config to the base class (e.g. `PlatformSupportPlugin` requires a `PlatformSupportPluginConfig` , `MetadataPlugin` requires a `MetadataPluginConfig`, etc.).

These specific configs implement the `IPluginConfig` interface, which looks like this:
```cs
public interface IPluginConfig
{
    PluginConfigItem[] Items { get; set; }

    void Reset();
}
```
As you can see, a config **must** include a field for any additional config items you may want to add. More on that [later](/Melora/plugin-development/configs.html#custom-config-items). The interface also requires a method to **reset** the config. However, you typically don't need to implement this yourself, as the specific plugin classes handle all the heavy lifting.


#### Example:
For instance, `PlatformSupportPluginConfig` **requires** a few parameters, such as the download quality, format, etc., which you **must** include.

When initializing your plugin, you can use the constructor of these configs. This allows you to pass in your **default config values** (used if the Melora client can't find any existing configs for your plugin or when the user resets your plugin config) and an **instance of an existing config** (if it exists).

Here’s what the constructor for a specific plugin config might look like:
```cs
public PlatformSupportPluginConfig(
    PluginConfigItem[] defaultItems,
    Quality defaultQuality,
    Format defaultFormat,
    int? defaultSearchResultsLimit,
    Sorting defaultSearchResultsSorting,
    bool defaultSearchResultsSortDescending,
    PlatformSupportPluginConfig? initialConfig = null)
```
And here's an example of how it could be initialized:
```cs
PlatformSupportPluginConfig pluginConfig = new(
    defaultItems:
    [
      new("Save Lyrics", "Whether to search & save lyrics", true),
      new("Access Token", "The access token required for xyz", "<SOME USER ACCESS TOKEN>")
    ],
    defaultQuality: Quality._160kbps,
    defaultFormat: Format.mp3,
    defaultSearchResultsLimit: null,
    defaultSearchResultsSorting: Sorting.Default,
    defaultSearchResultsSortDescending: false,
    initialConfig: existingUserConfig)
```


## Custom Config Items
While the default plugin configs cover many useful properties (e.g., download quality, format, etc., in `PlatformSupportPluginConfig`), you might need to **add** settings **specific** to **your** plugin, such as an access token, API key, or **other unique setting fields**.

### What Are Custom Config Items?
This is where the `PluginConfigItem` comes in. Every `IPluginConfig` includes an array of these items, allowing you to add **any custom** settings you need. When creating your config, simply pass the default values in the constructor. Melora will ensure these custom settings are populated in the user's config when your plugin is loaded.

#### The `PluginConfigItem` Constructor:
```cs
public PluginConfigItem(
    string name,
    string description,
    object value)
```
- **Name:** A short, clear name for your setting.
- **Description:** A brief description of what this setting does and how it affects your plugin.
- **Value:** The default value of your custom config item.

This approach allows you to extend the configuration options for your plugin, making it more **versatile** and **easier** for any users of your plugin.

::: warning Note
Melora currently supports only the following value types: `String`, `Int64`, and `Boolean`. Using other types could cause issues when displaying your config in the Melora settings menu.
:::

### How To Use Them?
To access custom settings in your plugin code, use the extension method `GetItem<T>(string name)` from `IPluginConfig`. This method **retrieves** and **casts** the value to the specified type `T`. If the item is not found or the type does not match, it throws a `PluginConfigInvalidItemException`.

```cs
string clientId = config.GetItem<string>("Client ID");
long artworkResolution = config.GetItem<long>("Artwork Resolution");
bool saveLyrics = config.GetItem<bool>("Save Lyrics");
```

## Handle Config Updates
It may happen that users change some of your plugin settings. While dynamic use of config values is straightforward, you might need to **reinitialize** components based on updated config values (like a **client** with a new **Client ID**).

To handle these situations, **all** specific plugin configs, such as `PlatformSupportPluginConfig` and `MetadataPluginConfig`, as well as `PluginConfigItem`, implement the `INotifyPropertyChanged` interface. This allows you to easily subscribe to property change events and update your plugin as needed.

#### Example:
```cs{4,13,16}
public SpotifyPlugin() : this(null, null)
{
    wrapper = new(Config, logger);
    Config.PropertyChanged += OnConfigPropertyChanged;
}

void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    switch (e.PropertyName)
    {
        case "Client ID":
        case "Client Secret":
            wrapper.ReAuthenticateClient();
            break;
        case "Genius Access Token":
            wrapper.ReAuthenticateGeniusClient();
            break;
    }
}
```
- `OnConfigPropertyChanged` method handles changes based on the `PropertyName` provided by the `PropertyChangedEventArgs`.
- If certain settings change (e.g., **"Client ID"** or **"Client Secret"**), you can reinitialize or re-authenticate components as needed.

::: info Note
You **don't need** to manually subscribe to each custom `PluginConfigItem`'s PropertyChanged event. They will automatically forward their `PropertyChanged` events, simplifying the process.
:::


## How Users Modify Your Config
Once your plugin is set up with configurable options, users can **easily** adjust these settings through Melora's interface. They simply navigate to the **Settings** page, find your plugin, and modify the configuration as needed.

#### Platform-Support Plugin Settings:
![](/guide/platform-support-configure.webp)

#### Metadata Settings:
![](/guide/metadata-configure.webp)
