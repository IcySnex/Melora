import { sidebar } from "vuepress-theme-hope";

const separator = {
  template: `<hr style="border-color: rgba(0, 0, 0, 0.1); margin: 20px;">`
}

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
      text: "Platform-Support",
      icon: "fe:music",
      link: "/guide/platform-support"
    },
    {
      text: "Metadata",
      icon: "tabler:tag-filled",
      link: "/guide/metadata"
    },
    {
      text: "Downloading",
      icon: "mingcute:arrow-down-fill",
      link: "/guide/downloading"
    },
    {
      text: "Lyrics",
      icon: "material-symbols:lyrics",
      link: "/guide/lyrics"
    },
    {
      text: "Updating",
      icon: "ic:baseline-update",
      link: "updating",
    },
  ],
  "/plugin-collection": [
    {
      text: "Platform-Support",
      icon: "fe:music",
      link: "/plugin-collection/platform-support",
    },
    {
      text: "Metadata",
      icon: "tabler:tag-filled",
      link: "/plugin-collection/metadata",
    }
  ]
});
