import { sidebar } from "vuepress-theme-hope";

export default sidebar({
  "/guide": [
    {
      text: "Introduction",
      icon: "material-symbols:info",
      link: "/guide/introduction"
    },
    {
      text: "Getting Started",
      icon: "fluent:lightbulb-16-filled",
      link: "/guide/getting-started"
    },
    {
      text: "Plugins",
      icon: "mingcute:plugin-2-fill",
      link: "/guide/plugins"
    },
    {
      text: "Searching",
      icon: "iconamoon:search-bold",
      link: "/guide/searching"
    },
    {
      text: "Downloading",
      icon: "fe:music",
      link: "/guide/downloading"
    },
    {
      text: "Lyrics",
      icon: "material-symbols:lyrics",
      link: "/guide/lyrics"
    }
  ]
});
