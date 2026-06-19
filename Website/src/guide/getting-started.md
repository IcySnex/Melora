---
title: Getting Started
icon: fluent:lightbulb-16-filled
order: 2
---

## Requirements
- **OS:** Windows 10 / 11
  - **Minimum Build:** 17763
  - **Target Build:** 19041
- **Architecture:** 64-bit / 32-bit / ARM64 (untested)


## Installation
- **Step 1:** Go to the Melora [GitHub Releases](https://github.com/IcySnex/Melora/releases) page.
- **Step 2:** Download the latest version for your architecture and extract it to a Melora folder.
- **Step 3:** If FFmpeg is not already installed on your system, download the [Windows binaries](https://github.com/System233/ffmpeg-msvc-prebuilt/releases/latest) as well.


## Post-Install Setup
After successfully installing Melora via the setup wizard, you'll need to adjust a few preferences on the settings page:

### FFmpeg
Melora uses FFmpeg internally to encode your downloaded tracks into your desired audio format. To configure the FFmpeg binary, follow these steps:
- **Step 1:** Open the Melora **Settings** page.
- **Step 2:** Expand the "Paths" section.
- **Step 3:** Click the "Set Executable" button next to the FFmpeg path.
- **Step 4:** Select the FFmpeg executable you previously downloaded.

![](/guide/getting-started-ffmpeg.webp)

### Other Paths
In the Settings menu, you can also specify the download location for your tracks and customize the file name structure using dynamic placeholders.

**Available Placeholders:**
* `{title}`, `{album}` - Tracl title and albums
* `{artists}` - Track artists (defaults to `Artist 1, Artist 2, ...`)
* `{release}` - Release date (defaults to `MM-dd-yyyy`)
* `{track}` / `{tracks}` - Album track number / Total track count
* `{disc}` / `{discs}` - Disc number / Total disc count
* `{pos}` / `{max}` - Download queue position / Total queue count

**Formatting Options:**
You can customize how numbers, dates, and artists are displayed by appending a colon (`:`) and a modifier inside the brackets. For a full list of formats, see the [official .NET Formatting Docs](https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types).

* **Numbers:** Add zeros to pad digits (e.g., `{track:000}` ➔ `001`, `001`, ...).
* **Dates:** Use standard date codes (e.g., `{release:yyyy}` ➔ `2026`, `{release:yy-MM}` ➔ `26-06`).
* **Artists:** Change how multiple artists are separated:
  * `{artists:first}` ➔ Only the main artist (`Artist 1`)
  * `{artists:&}` ➔ Joins with an ampersand (`Artist 1 & Artist 2`)
  * `{artists:-}` ➔ Joins with a hyphen (`Artist 1-Artist 2`)

**Folder Organization:**
To automatically organize your music into folders, use the backslash (`\`) character. For example, setting the file name to `"{artists}\{album}\{track:00} - {title}"` will save your track inside an album folder, which is nested inside an artist folder.

![](/guide/getting-started-otherpaths.webp)


### Handling Duplicate Tracks
When you attempt to download a track that already exists with the same name, Melora offers several ways to manage this situation. You can adjust the default behavior in the settings, choosing from three options:
- **Ask each time:** A popup will appear each time a duplicate is detected, prompting you to either overwrite the track or skip it. Note that this will pause the download queue until you provide input, which may interrupt bulk downloads.
- **Skip track:** The track will be automatically skipped, and a small warning notification will appear. The download queue will continue without interruption.
- **Overwrite track:** The existing track will be automatically overwritten, and the download queue will proceed without pausing.

![](/guide/getting-started-alreadyexists.webp)

---

::: warning Please notice
Plugins are required for Melora to function. They handle **searching**, **track downloads**, and **metadata writing**.

Without plugins, you won’t be able to use Melora. To learn more, refer to the [Plugins Guide](/Melora/guide/plugins.html) to get started.
:::
