<script lang="ts">
    import type { LangStat } from "./api/lang";
    import type { Paste, PasteStats, Pasty } from "./api/paste";
    import type { Settings } from "./api/settings";
    import PastyMeta from "./PastyMeta.svelte";
    import Tab from "./Tab.svelte";
    import { isLanguageMarkdown } from "./utils/markdown";
    import { browser } from "$app/environment";
    import type { Action } from "svelte/action";
    import { onMount } from "svelte";

    interface Props {
        paste: Paste;
        settings: Settings;
        pasteStats?: PasteStats;
        langStats: LangStat[];
        highlightedCode: string[];
        renderedMarkdown: { id: string; renderedMarkdown: string }[];
        historyId?: string;
    }

    let {
        paste,
        settings,
        pasteStats,
        langStats,
        highlightedCode,
        renderedMarkdown,
        historyId = undefined
    }: Props = $props();

    let activePastyId: string = $state(paste.pasties[0].id);
    let activePasty: Pasty = $state(paste.pasties[0]);

    $effect(() => {
        const p = paste.pasties.find((p) => p.id === activePastyId);
        if (p) activePasty = p;
    });

    let previewMarkdown: boolean[] = $state(paste.pasties.map(() => true));

    const setActiveTab = (id: string) => {
        activePastyId = id;
    };

    const focusMarkdownHeading: Action = () => {
        if (browser && window.location.hash) {
            const id = window.location.hash.slice(1);
            const element = document.getElementById(id);
            if (element) {
                const yOffset = -90;
                const y = element.getBoundingClientRect().top + window.scrollY + yOffset;

                window.scrollTo({ top: y, behavior: "instant" });
            }
        }
    };

    onMount(() => {
        const lineNumberElements = window.document.querySelectorAll(".line-number");

        const highlight = window.location.hash.match(/L(\d+)(?:-L(\d+))?/);

        let lastClickedLine: number | null = (highlight && highlight[1]) ? parseInt(highlight[1], 10) : null;

        for (const lineNumberElement of lineNumberElements) {
            lineNumberElement.addEventListener("click", (el: Event) => {
                const line = parseInt((el.currentTarget as HTMLElement).innerText, 10);

                if ((el as MouseEvent).shiftKey && lastClickedLine !== null) {
                    const start = Math.min(lastClickedLine, line);
                    const end = Math.max(lastClickedLine, line);

                    const highlighted = window.document.querySelectorAll(".line.highlight");
                    highlighted.forEach((hl) => {
                        hl.classList.remove("highlight", "first", "last");
                    });

                    for (let i = start; i <= end; i++) {
                        const element = window.document.querySelector(`.line[data-line="${i}"]`);
                        if (element) {
                            element.classList.add("highlight");
                            if (i === start) element.classList.add("first");
                            if (i === end) element.classList.add("last");
                        }
                    }

                    window.location.hash = `L${start}-L${end}`;
                } else {
                    const highlighted = window.document.querySelectorAll(".line.highlight");
                    highlighted.forEach((hl) => {
                        hl.classList.remove("highlight", "first", "last");
                    });

                    const element = window.document.querySelector(`.line[data-line="${line}"]`);
                    if (element) {
                        element.classList.add("highlight", "first", "last");
                    }

                    window.location.hash = `L${line}`;
                }

                lastClickedLine = line;
            });
        }

        if (highlight) {
            const start = parseInt(highlight[1], 10);
            const end = highlight[2] ? parseInt(highlight[2], 10) : start;

            for (let i = start; i <= end; i++) {
                const element = window.document.querySelector(`.line[data-line="${i}"]`);
                if (element) {
                    element.classList.add("highlight");
                    if (i === start) element.classList.add("first");
                    if (i === end) element.classList.add("last");
                }
            }

            const firstLine = window.document.querySelector(`.line[data-line="${start}"]`);
            if (firstLine) {
                const yOffset = -90;
                const y = firstLine.getBoundingClientRect().top + window.scrollY + yOffset;

                window.scrollTo({ top: y, behavior: "instant" });
            }
        }
    });
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
                                    bind:previewMarkdown={previewMarkdown[i]}
                                />
                            </div>
                        {/if}
                    </div>
                </div>

                {#if isLanguageMarkdown(pasty.language) && previewMarkdown[i]}
                    <div class="markdown" use:focusMarkdownHeading>
                        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                        {@html renderedMarkdown.find((p) => p.id === pasty.id)?.renderedMarkdown}
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
                            onclick={() => setActiveTab(pasty.id)}
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
                        bind:previewMarkdown={previewMarkdown[
                            paste.pasties.findIndex((p) => p.id === activePastyId)
                        ]}
                    />
                </div>
            {/if}
        </div>

        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        {#if isLanguageMarkdown(activePasty.language) && previewMarkdown[paste.pasties.findIndex((p) => p.id === activePastyId)]}
            <div class="markdown" use:focusMarkdownHeading>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                {@html renderedMarkdown.find((p) => p.id === activePasty.id)?.renderedMarkdown}
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
            margin-bottom: 1rem;
            overflow-x: auto;
        }

        :global(.shiki code) {
            border: none;
            font-size: $fs-normal;
            padding: 0;
            border-radius: 0;
            background-color: transparent;
            display: flex;
            flex-direction: column;
            width: max-content;
        }

        :global(.shiki code .line-number) {
            width: 1rem;
            margin-right: 1rem;
            display: inline-block;
            text-align: right;
            color: var(--color-bg3);
            font-size: $fs-normal;
            user-select: none;
            cursor: pointer;
            @include transition();

            &:hover {
                color: var(--color-fg);
            }
        }

        :global(.shiki .line) {
            @include transition();
        }

        :global(.shiki .line.highlight) {
            background-color: color-mix(in srgb, var(--color-secondary) 15%, transparent);
            width: 100%;
        }

        :global(.shiki .line.highlight.first) {
            border-top-left-radius: $border-radius;
            border-top-right-radius: $border-radius;
        }

        :global(.shiki .line.highlight.last) {
            border-bottom-left-radius: $border-radius;
            border-bottom-right-radius: $border-radius;
        }
    }

    .sticky {
        position: sticky;
        top: 0;
        z-index: 10;
    }
</style>
