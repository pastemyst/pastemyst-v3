<script lang="ts">
    import type { LangStat } from "./api/lang";
    import type { Paste, PasteStats, Pasty } from "./api/paste";
    import type { Settings } from "./api/settings";
    import Markdown from "./Markdown.svelte";
    import PastyMeta from "./PastyMeta.svelte";
    import Tab from "./Tab.svelte";
    import { isLanguageMarkdown } from "./utils/markdown";

    export let paste: Paste;
    export let settings: Settings;
    export let pasteStats: PasteStats | null;
    export let langStats: LangStat[];
    export let highlightedCode: string[];
    export let historyId: string | null = null;

    let activePastyId: string = paste.pasties[0].id;
    let activePasty: Pasty = paste.pasties[0];

    $: {
        const p = paste.pasties.find((p) => p.id === activePastyId);
        if (p) activePasty = p;
    }

    let previewMarkdownStacked: boolean[] = [];
    let previewMarkdownTabbed: boolean;

    const setActiveTab = (id: string) => {
        activePastyId = id;
    };
</script>

<div class="pasties">
    {#if settings.pasteView === "stacked"}
        {#each paste.pasties as pasty, i}
            <div class="pasty">
                <div class="sticky">
                    <div class="title flex row space-between center">
                        <span>{pasty.title || "untitled"}</span>

                        {#if pasteStats}
                            <div class="meta-stacked flex row center">
                                <PastyMeta
                                    {paste}
                                    {pasty}
                                    {langStats}
                                    {historyId}
                                    stats={pasteStats.pasties[pasty.id]}
                                    bind:previewMarkdown={previewMarkdownStacked[i]}
                                />
                            </div>
                        {/if}
                    </div>
                </div>

                {#if isLanguageMarkdown(pasty.language) && previewMarkdownStacked[i]}
                    <div class="markdown">
                        <Markdown content={pasty.content} />
                    </div>
                {:else}
                    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                    {@html highlightedCode[i]}
                {/if}
            </div>
        {/each}
    {:else}
        <div class="sticky">
            <div class="tabs flex row center">
                <div class="tabgroup flex row">
                    {#each paste.pasties as pasty}
                        <Tab
                            id={pasty.id}
                            isReadonly
                            title={pasty.title}
                            isActive={pasty.id === activePastyId}
                            on:click={() => setActiveTab(pasty.id)}
                        />
                    {/each}
                </div>
            </div>

            {#if pasteStats}
                <div class="meta-tabbed">
                    <PastyMeta
                        {paste}
                        pasty={activePasty}
                        {langStats}
                        {historyId}
                        stats={pasteStats.pasties[activePastyId]}
                        bind:previewMarkdown={previewMarkdownTabbed}
                    />
                </div>
            {/if}
        </div>

        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        {#if isLanguageMarkdown(activePasty.language) && previewMarkdownTabbed}
            <div class="markdown">
                <Markdown content={activePasty.content} />
            </div>
        {:else}
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            {@html highlightedCode[paste.pasties.findIndex((p) => p.id === activePastyId)]}
        {/if}
    {/if}
</div>

<style lang="scss">
    .pasties {
        margin-top: 1rem;

        .tabs {
            width: 100%;
            box-sizing: border-box;
            background-color: var(--color-bg2);
            border-radius: $border-radius $border-radius 0 0;
            border-bottom: 1px solid var(--color-bg2);

            .tabgroup {
                flex-wrap: wrap;
                width: 100%;

                :global(.tab) {
                    flex-grow: 0;
                }
            }
        }

        .meta-tabbed {
            background-color: var(--color-bg1);
            padding: 0.25rem;
            border: 1px solid var(--color-bg2);
            border-top: none;
        }

        .pasty {
            margin-bottom: 2rem;

            &:last-child {
                margin-bottom: 0;
            }

            .title {
                background-color: var(--color-bg1);
                border-radius: $border-radius $border-radius 0 0;
                border-bottom: 1px solid var(--color-primary);
                width: 100%;
                box-sizing: border-box;

                span {
                    padding: 0.5rem 1rem;
                }

                .meta-stacked {
                    padding-right: 1rem;
                }
            }
        }

        .markdown {
            padding: 0rem 2rem;
        }

        :global(.shiki),
        .markdown {
            border-bottom-left-radius: $border-radius;
            border-bottom-right-radius: $border-radius;
            border: 1px solid var(--color-bg2);
            border-top: none;
            margin: 0;
            overflow-x: auto;
        }

        :global(.shiki code) {
            border: none;
            font-size: $fs-normal;
            padding: 0;
            border-radius: 0;
            background-color: transparent;
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
    }

    .sticky {
        position: sticky;
        top: 0;
        z-index: 10;
    }
</style>
