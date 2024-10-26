<script lang="ts">
    import { marked } from "marked";
    import { markedHeadingAnchorExtension } from "./marked-heading-anchor";
    import { browser } from "$app/environment";
    import { onMount } from "svelte";

    export let content: string;

    marked.use(markedHeadingAnchorExtension());

    $: markdownHtml = marked.parse(content, { gfm: true }) as string;

    onMount(() => {
        if (browser && window.location.hash) {
            const id = window.location.hash.slice(1);
            const element = document.getElementById(id);
            if (element) {
                element.scrollIntoView();
            }
        }
    });
</script>

<div class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html markdownHtml}
</div>
