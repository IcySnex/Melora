---
title: Platform-Support
icon: fe:music
order: 1
---

Here you can find officially tested Platform-Support plugins for Melora, ensuring both **quality** and **safety**.

If you're unfamiliar with what Platform-Support plugins are, please refer to the [Platform-Support Guide](/Melora/guide/platform-support.html). If you don't know how to install a plugin, please refer to the [Installing A Plugin Guide](/Melora/guide/plugins#installing-a-plugin).


<PluginBundleCollection :manifests="manifestsData"/>

<script setup>
import manifestsData from "@plugin-manifests-platform-support"
</script>