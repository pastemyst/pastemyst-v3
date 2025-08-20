<script lang="ts">
    import type { PageServerData } from "./$types";

    interface Props {
        data: PageServerData;
    }

    let { data = $bindable() }: Props = $props();
</script>

<section class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html data.renderedMarkdown}
</section>
