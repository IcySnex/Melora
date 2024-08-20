---
title: Metadata
icon: tabler:tag-filled
---

Here you can find officially tested Metadata plugins for Melora, ensuring both **quality** and **safety**.

If you're unfamiliar with what Metadata plugins are, please refer to the [Metadata Guide](/Melora/guide/metadata.html). If you don't know how to install a plugin, please refer to the [Installing A Plugin Guide](/Melora/guide/plugins#installing-a-plugin).


<PluginBundleCollection :manifests="manifestsData"/>

<script setup>
import manifestsData from "@plugin-manifests-metadata"
</script>