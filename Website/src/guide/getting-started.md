---
title: Getting Started
icon: fluent:lightbulb-16-filled
---

## System Requirements
- Windows 10 / 11
  - *Minimum Build:* 17763
  - *Target Build:* 19041
- 64-Bit / 32-Bit / ARM-64 (untested)


## Installation
- **Step 1:** Go to the [GitHub Releases](https://github.com/IcySnex/Melora/releases) page.
- **Step 2:** Download the latest installer for your architecture, then follow the setup wizard.
- **Step 3:** If FFmpeg is not already installed on your system, download the [Windows binaries](https://ffbinaries.com/downloads) as well.


## Post-Install Setup
After successfully installing Melora via the setup wizard, you'll need to adjust a few preferences on the settings page:

### FFmpeg
Melora uses FFmpeg internally to encode your downloaded tracks into your desired audio format. To configure the FFmpeg binary, follow these steps:
- **Step 1:** Open the Melora settings page.
- **Step 2:** Expand the "Paths" section.
- **Step 3:** Click the "Set Executable" button next to the FFmpeg path.
- **Step 4:** Select the FFmpeg executable you previously downloaded.

![](/guide/ffmpeg.webp)

### Other Paths
In the settings menu, you can also specify the download location for your tracks. Additionally, you can customize the file name by embedding track information using placeholders like *{title}*, *{artists}*, *{album}*, and *{release}*.

To organize your tracks into a folder structure, use the '\' character. For example, setting the file name as *"{artists}\{album}\{title}"* would save the track within a folder named after the album, which is nested inside a folder named after the artist. This allows for a neatly organized music library.

![](/guide/otherpaths.webp)


### Handling Duplicate Tracks
When you attempt to download a track that already exists with the same name, Melora offers several ways to manage this situation. You can adjust the default behavior in the settings, choosing from three options:
- **Ask each time:** A popup will appear each time a duplicate is detected, prompting you to either overwrite the track or skip it. Note that this will pause the download queue until you provide input, which may interrupt bulk downloads.
- **Skip track:** The track will be automatically skipped, and a small warning notification will appear. The download queue will continue without interruption.
- **Overwrite track:** The existing track will be automatically overwritten, and the download queue will proceed without pausing.

![](/guide/alreadyexists.webp)
