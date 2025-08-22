<script lang="ts">
    import type { PageServerData } from "./$types";

    interface Props {
        data: PageServerData;
    }

    let { data = $bindable() }: Props = $props();
</script>

<svelte:head>
    <title>pastemyst | copyright infringment policy</title>
    <meta property="og:title" content="pastemyst | copyright infringment policy" />
    <meta property="twitter:title" content="pastemyst | copyright infringment policy" />
</svelte:head>

<section class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html data.renderedMarkdown}
</section>
