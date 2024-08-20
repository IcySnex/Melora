import { defineUserConfig } from "vuepress";
import { getDirname, path } from "vuepress/utils";
import theme from "./theme.js";

const __dirname = getDirname(import.meta.url);

export default defineUserConfig({
  base: "/Melora/",
  lang: "en-US",

  title: "Melora",
  description: "Melora allows you to download all your music from any platform using custom plugins.",

  head: [
    ["link", { rel: "icon", href: "/Melora/favicon.png" }]
  ],

  alias: {
    "@plugin-manifests-platform-support": path.resolve(__dirname, "../../data/plugin-manifests/platform-support.json"),
    "@plugin-manifests-metadata": path.resolve(__dirname, "../../data/plugin-manifests/metadata.json")
  },

  theme
});
