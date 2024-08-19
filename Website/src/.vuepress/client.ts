import { defineClientConfig } from "vuepress/client";
import ProjectPanel from "./components/PluginBundleDetail.vue";

export default defineClientConfig({
  enhance: ({ app }) => {
    app.component("PluginBundleDetail", ProjectPanel);
  },
});