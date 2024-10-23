<script lang="ts">
    import { marked } from "marked";
    import { markedHeadingAnchorExtension } from "./marked-heading-anchor";

    export let content: string;

    marked.use(markedHeadingAnchorExtension());

    let markdownHtml = marked.parse(content, { gfm: true }) as string;
</script>

<div class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html markdownHtml}
</div>
