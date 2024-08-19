<script setup lang="ts">
import { computed } from "vue";
import { useRouteLocale } from "vuepress/client";
import {
  isLinkHttp,
  isPlainObject,
  useLocaleConfig,
} from "@vuepress/helper/client";
import { GitHubIcon } from "vuepress-shared/client";

import Icon from "@theme-hope/components/HopeIcon";
import { normalizePackageName } from "../utils/index.js";

const props = withDefaults(
  defineProps<{
    name?: string;
    description?: string;
    url: string;
  }>(),
  {
    name: "",
    description: "",
  }
);


const getUrl = (url: string): string =>
  isLinkHttp(url) ? url : `https://${url}`;


const pluginBundleName = computed(() => {
  const name = props.name;
  if (isPlainObject(name)) return name || "N/A";
});

const pluginBundleDescription = computed(() => {
  const description = props.description;
  if (isPlainObject(description)) return description || "N/A";
});

const pluginBundleLink = computed(() => {
  return getUrl(props.url);
});


const open = () => {
  window.open(pluginBundleLink.value, "_blank");
};
</script>

<template>
  <div class="plugin-bundle-card" :class="{ recommend }" @click="open">
    <!-- <a
      class="plugin-bundle-card-source"
      :href="projectSource"
      :title="locale.source"
      target="_blank"
      @click.stop
    >
      <GitHubIcon class="source-icon" />
    </a> -->
    <h3 class="plugin-bundle-header" :id="packageName">
      {{ pluginBundleName }}
    </h3>
    <p
      v-if="pluginBundleDescription"
      class="plugin-bundle-description"
      v-html="pluginBundleDescription"
    />
  </div>
</template>

<style lang="scss">
.plugin-bundle-card {
  position: relative;

  flex-basis: calc(50% - 3rem);

  margin: 0.5rem;
  padding: 1rem;
  border-radius: 0.5rem;

  box-shadow: 1px 1px 8px var(--card-shadow);

  cursor: pointer;

  transition:
    box-shadow var(--color-transition),
    box-shadow var(--transform-transition);

  @media (max-width: hope-config.$pad) {
    flex-basis: calc(100% - 2rem);
    margin: 0.5rem 0;
    font-size: 0.9rem;
  }

  &:hover {
    background-color: var(--bg-color-secondary);
    box-shadow: 0 2px 12px 0 var(--card-shadow);
  }

  &.recommend {
    flex-basis: calc(50% - 3.25rem);

    border: 0.125rem solid transparent;
    background-clip: padding-box, border-box;
    background-origin: padding-box, border-box;
    background-image: linear-gradient(
        to right,
        var(--bg-color),
        var(--bg-color)
      ),
      linear-gradient(120deg, #f6d365, #fda085);

    @media (max-width: hope-config.$pad) {
      flex-basis: calc(100% - 2.25rem);
      margin: 0.5rem 0;
      font-size: 0.9rem;
    }

    &:hover {
      background-image: linear-gradient(
          to right,
          var(--bg-color-secondary),
          var(--bg-color-secondary)
        ),
        linear-gradient(-120deg, #f6d365, #fda085);
    }
  }

  .plugin-bundle-header {
    margin: 0.25rem 0 0.5rem !important;
    color: var(--text-color-light);
    font-weight: bold;
    font-size: 1.3rem;

    @media (max-width: hope-config.$pad) {
      font-size: 1.2rem;
    }
  }

  .plugin-bundle-desc {
    margin: 0.5rem 0;
    color: var(--text-color-lighter);
    line-height: 1.4;
  }
}
</style>