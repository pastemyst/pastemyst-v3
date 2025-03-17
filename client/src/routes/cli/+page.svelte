<script lang="ts">
    import type { PageServerData } from "./$types";

    interface Props {
        data: PageServerData;
    }

    let { data = $bindable() }: Props = $props();
</script>

<svelte:head>
    <title>pastemyst | cli tool</title>
</svelte:head>

<section class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html data.renderedMarkdown}
</section>

<style lang="scss">
    .markdown {
        padding-left: 2rem;
        padding-right: 2rem;

        :global(pre) {
            background-color: var(--color-bg) !important;
        }

        :global(code) {
            padding: 0;
        }
    }
</style>
