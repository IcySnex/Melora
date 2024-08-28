---
title: Platform-Support
icon: fe:music
order: 4
---

This guide will walk you through the process of creating your own Melora Platform-Support plugins by using the integration of SoundCloud with Melora as an example.
It is easily adaptable to any other platform as well though.

If you're new to Platform-Support plugins, please refer to the [Beginner's Guide](/Melora/guide/platform-support.html) for an introduction.

---
::: info Suggestion
Before you begin, it's helpful to organize your plugin bundle with the following structure:
```
└─ Melora.PlatformSupport.<NAME>
   ├─ Internal
   │  └─ ...
   ├─ Manifest.json
   └─ <NAME>Plugin.cs
```
- **Root:** It's recommended to place all plugins *(classes that implement IPlugin)* in the **root** of your plugin bundle. The manifest should also be here.
- **Internal:** Any additional components *(like models, helper classes...)* required internally should be organized within an **internal** namespace.
This structure keeps your plugin **organized** and ensures that your main functionalities are **easily accessible** while keeping auxiliary components **neatly separated**.
:::


## 1. Setup
Before diving into coding your new shiny Melora plugin, it's important to first follow the plugin development [Getting Started Guide](/Melora/plugin-development/getting-started.html). This guide will walk you through everything you need to set up your IDE, project, and plugin bundle manifest, ensuring you're fully prepared to start developing your plugin.

## 2. Implementing Your Plugin
Now for the fun part *— coding your plugin*! Here's how you can get started:

### 2.1 Create the Plugin Class
- Create a new class called `<NAME>Plugin.cs` in the root of your project.
- This class should inherit from `PlatformSupportPlugin`, which itself implements the `IPlugin` interface.
```cs
public class SoundCloudPlugin : PlatformSupportPlugin
{
    // YOUR PLUGIN CODE
}
```

### 2.2 Constructors
The `PlatformSupportPlugin` class **requires** a few key pieces of information from your plugin, which are passed through its constructor. Here’s a breakdown of the parameters:
| Parameter | Description |
| --- | --- |
| `string` name | The name of the plugin. This will be used to display your plugin in the Melora client UI. |
| `string` iconPathData | The svg path data for the plugin icon. This will be used to display your plugin in the Melora client UI. |
| `PlatformSupportPluginConfig` config | The config the plugin gets initialized with. |
| `ILogger<IPlugin>?` logger | An optional logger. |

- Your plugin's constructor will need to pass these parameters when calling the **base class constructor** to properly initialize the `PlatformSupportPlugin`.

---

The Melora client will attempt to use various constructors based on your implementation and if a config is already saved for your plugin. Thats why you need to implement multiple constructors.

- Here are the required constructors if you plan on using logging in your plugin:
```cs
public SoundCloudPlugin(PlatformSupportPluginConfig? config, ILogger<IPlugin> logger) : base(
    name: "SoundCloud",
    iconPathData: "",
    config: new(
        defaultItems:
        [
            new("Save Lyrics", "Whether to search & save lyrics", true),
        ],
        defaultQuality: Quality._160kbps,
        defaultFormat: Format.mp3,
        defaultSearchResultsLimit: null,
        defaultSearchResultsSorting: Sorting.Default,
        defaultSearchResultsSortDescending: false,
        initialConfig: config),
    logger: logger)
{ }

public SoundCloudPlugin(ILogger<IPlugin> logger) : this(null, logger)
{ }
```

- If you don’t need logging in your plugin, use these constructors:
```cs
public SoundCloudPlugin(PlatformSupportPluginConfig? config) : base(
    name: "SoundCloud",
    iconPathData: "",
    config: new(
        defaultItems:
        [
            new("Save Lyrics", "Whether to search & save lyrics", true),
        ],
        defaultQuality: Quality._160kbps,
        defaultFormat: Format.mp3,
        defaultSearchResultsLimit: null,
        defaultSearchResultsSorting: Sorting.Default,
        defaultSearchResultsSortDescending: false,
        initialConfig: config),
    logger: null)
{ }

public SoundCloudPlugin() : this(null)
{ }
```
::: warning Note
It is **always recommended** to use logging in your plugin. This makes debugging **easier** if a user ever encounters any issues.

Especially since it's very easy to implement logging with Melora's plugin infrastructure.
::: 