---
title: Publishing
icon: material-symbols:public
order: 7
---

After finishing your Melora plugin, you'll likely want to **share** it with others!

While you can simply upload it to GitHub, there's also the option to **add** your plugin to the [official Melora Plugin Collection](/Melora/plugin-collection/). This collection features **safe**, **high-quality** plugins that are easily accessible to all Melora users.


## Requirements
Before submitting your plugin to the official Melora Plugin Collection, please thoroughly test it to ensure everything works as expected. This helps prevent issues and ensures a smooth experience for users.

Also make sure your plugin bundle includes a valid [Manifest](/Melora/plugin-development/manifest.html) with a **direct download link** set.


## Submitting
Submitting your Melora plugin is straightforward. Follow these steps:

### Step 1: Host Your Manifest
To ensure the Plugin Collection always displays the **latest information** about your plugin bundle, you need to **host** the manifest file **online**.

If you’ve uploaded your plugin’s source code to GitHub, you should already have a manifest URL for the plugin bundle. For example, your manifest URL might look like this:

`https://raw.githubusercontent.com/<Username>/<RepoName>/main/<SolutionName>/Manifest.json`.

### Step 2: Create a Pull Request
- Visit the [Melora GitHub repository](https://github.com/IcySnex/Melora).
- Create a pull request to add your manifest URL to the **appropriate** JSON file in the [Plugin Collection Data directory](https://github.com/IcySnex/Melora/tree/main/Website/data/plugin-manifestsUrls).

### Step 3: Validation and Approval
After submitting your pull request, it will be **reviewed**. Once it’s merged, your plugin will **automatically** appear in the [official Melora Plugin Collection](/Melora/plugin-collection/) on the Melora website.


## Updating
Since you have hosted the Plugin Bundle Manifest online, **any changes** you make to it will be reflected **instantly** in the Melora Plugin Collection.

This means you can update your plugin's information, features, or dependencies, and users will see the latest version **without** needing to **resubmit** the plugin.