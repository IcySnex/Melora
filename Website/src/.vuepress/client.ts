import { defineClientConfig } from "vuepress/client";
import PluginBundleCollection from "./components/PluginBundleCollection.vue";

export default defineClientConfig({
  enhance: ({ app }) => {
    app.component("PluginBundleCollection", PluginBundleCollection);
  },
});