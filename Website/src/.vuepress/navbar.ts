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
    link: "/guide/",
    prefix: "/guide/",
    children: [
      {
        text: "Introduction",
        icon: "material-symbols:info",
        link: "introduction",
      },
      {
        text: "Getting Started",
        icon: "fluent:lightbulb-16-filled",
        link: "getting-started",
      },
      {
        text: "Plugins",
        icon: "mingcute:plugin-2-fill",
        link: "plugins",
      },
      {
        text: "Platform-Support",
        icon: "fe:music",
        link: "platform-support",
      },
      {
        text: "Metadata",
        icon: "tabler:tag-filled",
        link: "metadata"
      },
      {
        text: "Downloading",
        icon: "mingcute:arrow-down-fill",
        link: "downloading",
      },
      {
        text: "Lyrics",
        icon: "material-symbols:lyrics",
        link: "lyrics",
      },
    ],
  },
  {
    text: "Plugin Collection",
    icon: "mingcute:plugin-2-fill",
    link: "/plugin-collection/",
    prefix: "/plugin-collection/",
    children: [
      {
        text: "Platform-Support",
        icon: "fe:music",
        link: "platform-support",
      },
      {
        text: "Metadata",
        icon: "tabler:tag-filled",
        link: "metadata",
      },
    ]
  }
]);
