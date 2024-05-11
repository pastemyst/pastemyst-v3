<script lang="ts">
    import DOMPurify from "dompurify";
    import { marked } from "marked";
    import { gfmHeadingId } from "marked-gfm-heading-id";

    export let content: string;

    marked.use(gfmHeadingId());

    let markdownHtml = marked.parse(content, { gfm: true }) as string;
</script>

<!-- eslint-disable-next-line svelte/no-at-html-tags -->
{@html DOMPurify.sanitize(markdownHtml)}

<style lang="scss">
    :global(.markdown) {
        padding: 1rem 2rem;
        text-align: justify;
        line-height: 1.5rem;
        font-size: $fs-normal;

        :global(h1) {
            margin-top: 0;
        }

        :global(h1:first-child) {
            margin-top: 1rem;
        }

        :global(h1),
        :global(h2),
        :global(h3) {
            border-bottom: 1px solid var(--color-bg2);
            padding-bottom: 0.5rem;
            margin-top: 2rem;
        }

        :global(blockquote) {
            border-left: 4px solid var(--color-secondary);
            padding-left: 1rem;
            margin-left: 0;
        }

        :global(pre) {
            background-color: var(--color-bg1);
            border-radius: $border-radius;
            border: 1px solid var(--color-bg2);
            line-height: initial;
            padding: 1rem;

            :global(code) {
                border: none;
                background-color: initial;
            }
        }

        :global(img) {
            max-width: 100%;
        }
    }
</style>
