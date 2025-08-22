<script lang="ts">
    import type { PageServerData } from "./$types";

    interface Props {
        data: PageServerData;
    }

    let { data = $bindable() }: Props = $props();
</script>

<svelte:head>
    <title>pastemyst | terms of service</title>
    <meta property="og:title" content="pastemyst | terms of service" />
    <meta property="twitter:title" content="pastemyst | terms of service" />
</svelte:head>

<section class="markdown">
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html data.renderedMarkdown}
</section>
