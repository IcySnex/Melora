---
title: Manifest
icon: ant-design:setting-filled
order: 3
---

Each plugin bundle has to contain a so called Manifest. This contains all the necessary information required to display and load it.


## Structure
```json
{
  "Name": <string>,
  "Description": <string>,
  "Author": <string>,
  "ApiVersion": <string>,
  "LastUpdatedAt": <DateTime>,
  "SourceUrl": <string>,
  "DownloadUrl": <string>,
  "PluginKinds": <array> of type <int>,
  "EntryPoint": <string>,
  "Dependencies": <array> of type <string>
}
```
| Property | Description |
| --- | --- |
| `Name` | The display name of your plugin. Choose something simple and easily recognizable to help users organize their plugins. |
| `Description` | A brief description of what your plugin bundle does and how users can benefit from it. |
| `Author` | Your developer name, so users know who to credit. |
| `ApiVersion` | Specifies the version of the `Melora.Plugins` API your plugin bundle uses. This helps ensure **compatibility** with the user's Melora client. You can find the version in the .csproj file of the `Melora.Plugins` project. |
| `LastUpdatedAt` | Indicates when your plugin was last updated. |
| `SourceUrl` | A link to the plugin's source code, website, or any relevant online resource. |
| `DownloadUrl` | Used only if you're publishing your plugin bundle to the [official Melora Plugin Collection](/Melora/plugin-collection/). This should be the **direct** download link.. |
| `PluginKinds` | Describes the types of plugins your bundle contains. For a list of plugin types and their corresponding keys, see [here](/Melora/plugin-api-reference/Melora.Plugins/Enums/PluginKind.html) |
| `EntryPoint` | The **main assembly** of your plugin bundle. This assembly is loaded by the Melora client and should contain all your public plugins. |
| `Dependencies` | Lists any **additional** libraries used by your plugins. These will be loaded **alongside** the entry point by the Melora client. |

::: warning Note
While the `PluginKinds` field **supports** multiple kinds *— meaning you can include both **Platform-Support** and **Metadata** plugins in the same bundle —* it's generally **not** recommended.

Bundling different types together reduces flexibility for users who might only want to install one of them. 
:::


## Example
This is the official manifest of the [Spotify Plugin](https://github.com/IcySnex/Melora/blob/main/Plugins/Melora.PlatformSupport.Spotify/Manifest.json) for Melora:
```json
{
  "Name": "Spotify",
  "Description": "Adds support to search & download tracks, albums, playlists & artists from Spotify.",
  "Author": "IcySnex",
  "ApiVersion": "1.0.0",
  "LastUpdatedAt": "2024-08-20T14:26:00Z",
  "SourceUrl": "https://github.com/IcySnex/Melora/tree/main/Plugins/Melora.PlatformSupport.Spotify",
  "DownloadUrl": "https://github.com/IcySnex/Melora/releases/download/v1.0.0-stable/Melora.PlatformSupport.Spotify.mlr",
  "PluginKinds": [
    0 // Platform-Support
  ],
  "EntryPoint": "Melora.PlatformSupport.Spotify.dll",
  "Dependencies": [
    "SpotifyAPI.Web.dll"
  ]
}
```
To get more examples, please see the [GitHub project](https://github.com/IcySnex/Melora).