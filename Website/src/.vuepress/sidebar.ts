import { sidebar } from "vuepress-theme-hope";

const separator = {
  template: `<hr style="border-color: rgba(0, 0, 0, 0.1); margin: 20px;">`
}

export default sidebar({
  "/guide": "structure",
  "/plugin-collection": "structure",
  "/plugin-development": "structure",
  "/plugin-api-reference": "structure"
});
