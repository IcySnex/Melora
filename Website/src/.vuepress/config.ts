import { defineUserConfig } from "vuepress";

import theme from "./theme.js";

export default defineUserConfig({
  base: "/Melora/",
  lang: "en-US",

  title: "Melora",
  description: "Melora allows you to download all your music from any platform using custom plugins.",

  head: [
    ["link", { rel: "icon", href: "/favicon.png" }]
  ],

  plugins: [

  ],

  theme
});