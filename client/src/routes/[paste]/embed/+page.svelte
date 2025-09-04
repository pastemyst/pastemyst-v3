<script lang="ts">
    import { colorIsDark } from "$lib/utils/color";
    import type { PageServerData } from "./$types";

    interface Props {
        data: PageServerData;
    }

    let { data }: Props = $props();

    const getLangColor = (lang: string): string | undefined => {
        return data.langStats.find((l) => l.language.name === lang)?.language.color;
    };
</script>

<svelte:head>
    <script src="/static/scripts/iframeResizer.contentWindow.js"></script>
</svelte:head>

<div id="pastemyst-{data.paste.id}" class="pastemyst-embed">
    <div class="pastemyst-title">
        <a href="/{data.paste.id}" target="_blank">{data.paste.title}</a>
        <p class="pastemyst-powered-by">powered by <a href="/" target="_blank">pastemyst</a></p>
    </div>

    {#each data.paste.pasties as pasty, i (pasty.id)}
        <div class="pastemyst-pasty">
            <div class="pastemyst-pasty-title">
                <p>{pasty.title}</p>
                <p
                    class="pastemyst-pasty-language"
                    style="background-color: {getLangColor(pasty.language) ?? 'var(--color-fg)'};"
                    class:dark={colorIsDark(getLangColor(pasty.language) ?? "#ffffff")}
                >
                    {pasty.language}
                </p>
            </div>
            <div class="pastemyst-pasty-content">
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                {@html data.highlightedCode[i]}
            </div>
        </div>
    {/each}
</div>

<style lang="scss">
    .pastemyst-embed {
        font-family: $font-stack;
        font-size: $fs-normal;

        a {
            text-decoration: none;
        }

        p {
            margin: 0;
        }

        .pastemyst-title {
            display: flex;
            justify-content: space-between;
            align-items: center;
            background-color: var(--color-bg1);
            color: var(--color-fg);
            padding: 0.5rem 1rem;
            border-radius: $border-radius;

            .pastemyst-powered-by {
                font-size: $fs-small;
                color: var(--color-bg3);
            }
        }

        .pastemyst-pasty {
            margin-top: 1rem;

            .pastemyst-pasty-title {
                display: flex;
                justify-content: space-between;
                align-items: center;
                background-color: var(--color-bg1);
                color: var(--color-fg);
                padding: 0.5rem 1rem;
                border-top-left-radius: $border-radius;
                border-top-right-radius: $border-radius;
                border-bottom: 1px solid var(--color-primary);
            }

            .pastemyst-pasty-language {
                border-radius: $border-radius;
                padding: 0 0.25rem;
            }

            :global(.shiki code) {
                counter-reset: step;
                counter-increment: step 0;
            }

            :global(.shiki code .line::before) {
                content: counter(step);
                counter-increment: step;
                width: 1rem;
                margin-right: 1rem;
                display: inline-block;
                text-align: right;
                color: var(--color-bg3);
                font-size: $fs-normal;
            }

            .pastemyst-pasty-content :global(pre) {
                margin: 0;
                padding: 0;
                background-color: var(--color-bg1) !important;
                border: none;
                padding: 0.25rem;
                border-bottom-left-radius: $border-radius;
                border-bottom-right-radius: $border-radius;
                font-size: $fs-medium;
                line-height: 1.1rem;
            }

            .pastemyst-pasty-content :global(code) {
                border: none;
                background-color: var(--color-bg1) !important;
                padding: 0;
            }
        }
    }
</style>
