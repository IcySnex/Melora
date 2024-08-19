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
    "@platform-support-collection-config": path.resolve(
      __dirname,
      "../../config/plugin-collection/platform-support.json",
    ),
  },

  theme
});
