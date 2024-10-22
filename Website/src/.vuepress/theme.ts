import { hopeTheme } from "vuepress-theme-hope";

import navbar from "./navbar.js";
import sidebar from "./sidebar.js";

export default hopeTheme({
  hostname: "localhost",
  docsDir: "src",

  author: {
    name: "IcySnex",
    url: "https://github.com/IcySnex",
  },
  repo: "IcySnex/Melora",

  iconAssets: "iconify",
  logo: "logo.png",

  navbar,
  sidebar,

  footer: "Made with ♥️ in Germany",
  copyright: "<a href='/Melora/license.html'>Copyright (C) 2024 IcySnex</a>",
  displayFooter: true,
  breadcrumb: false,


  plugins: {
    search: {
      locales: {
        "/": {
          placeholder: "Search"
        }
      }
    },
    shiki: {
      theme: "tokyo-night",
      notationHighlight: true
    },
    mdEnhance: {
      align: true,
      attrs: true,
      component: true,
      demo: true,
      include: true,
      mark: true,
      plantuml: true,
      spoiler: true,
      stylize: [
        {
          matcher: "Recommended",
          replacer: ({ tag }) => {
            if (tag === "em")
              return {
                tag: "Badge",
                attrs: { type: "tip" },
                content: "Recommended",
              };
          },
        },
      ],
      sub: true,
      sup: true,
      tasklist: true,
      vPre: true,
    },
  }
});
