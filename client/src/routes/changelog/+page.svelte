<script lang="ts">
    import Markdown from "$lib/Markdown.svelte";
    import type { PageData } from "./$types";

    export let data: PageData;
</script>

<svelte:head>
    <title>pastemyst | changelog</title>
</svelte:head>

<section>
    <h1>changelog</h1>

    <h3>list of all released versions of pastemyst, along with their changelogs.</h3>

    {#if data.releases.length === 0}
        <p>
            There was an error fetching the changelog from the server. Try again later, or
            <a href="/contact">contact</a>
            the owner. You can also view the changelog
            <a href="https://github.com/pastemyst/pastemyst-v3" rel="external">on github</a>.
        </p>
    {:else}
        {#each data.releases as release}
            <div class="release">
                <div class="flex row center space-between">
                    <h4><a href={release.url} rel="external">{release.title}</a></h4>

                    {#if release.isPrerelease}
                        <span class="alpha">alpha-release</span>
                    {/if}
                </div>

                <p class="published-on">
                    Published on: {new Date(release.releasedAt).toDateString()}
                </p>

                <p class="content">
                    <Markdown content={release.content} />
                </p>
            </div>
        {/each}
    {/if}
</section>

<style lang="scss">
    h1 {
        margin-top: 0;
    }

    h3 {
        font-weight: normal;
        font-size: $fs-normal;
    }

    .release {
        margin-top: 2rem;
        background-color: var(--color-bg);
        padding: 0.5rem 1rem;
        border-radius: $border-radius;
        border: 1px solid var(--color-bg2);
    }

    h4 {
        font-size: $fs-large;
        font-weight: normal;
        margin: 0;
        margin-right: 2rem;
    }

    .alpha {
        border: 1px solid var(--color-secondary);
        padding: 0.25rem 0.5rem;
        border-radius: $border-radius;
        font-size: $fs-small;
    }

    .published-on {
        font-size: $fs-small;
    }

    .content {
        margin-top: 1.5rem;
        font-size: $fs-normal;
    }
</style>
