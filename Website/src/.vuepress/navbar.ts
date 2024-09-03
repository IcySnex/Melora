import { navbar } from "vuepress-theme-hope";

export default navbar([
  {
    text: "Home",
    icon: "material-symbols:home",
    link: "/",
  },
  {
    text: "Guide",
    icon: "ph:chat-fill",
    link: "/guide/getting-started",
    activeMatch: "^/guide/"
  },
  {
    text: "Plugin Development",
    icon: "mingcute:code-fill",
    link: "/plugin-development/getting-started",
    activeMatch: "^/plugin-development/"
  },
  {
    text: "Plugin Collection",
    icon: "mingcute:plugin-2-fill",
    link: "/plugin-collection/",
    activeMatch: "^/plugin-collection/",
    children: [
      {
        text: "Platform-Support",
        icon: "fe:music",
        link: "/plugin-collection/platform-support",
      },
      {
        text: "Metadata",
        icon: "tabler:tag-filled",
        link: "/plugin-collection/metadata",
      },
    ]
  }
]);
