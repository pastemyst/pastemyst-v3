<script lang="ts">
    import type { LangStat } from "./api/lang";
    import type { Paste, PasteStats, Pasty } from "./api/paste";
    import type { Settings } from "./api/settings";
    import PastyMeta from "./PastyMeta.svelte";
    import Tab from "./Tab.svelte";
    import { isLanguageMarkdown } from "./utils/markdown";
    import { browser } from "$app/environment";
    import type { Action } from "svelte/action";
    import { onMount, tick } from "svelte";
    import { themeStore } from "./stores.svelte";

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

    themeStore.subscribe(async (theme) => {
        if (!theme) return;

        for (const [i, pasty] of paste.pasties.entries()) {
            const res = await fetch("/internal/highlight", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    content: pasty.content,
                    language: pasty.language,
                    wrap: settings.textWrap,
                    theme: theme.name,
                    showLineNumbers: true
                })
            });

            highlightedCode[i] = await res.text();
        }

        highlightedCode = [...highlightedCode];
    });

    let previewMarkdown: boolean[] = $state(paste.pasties.map(() => true));

    const setActiveTab = async (id: string) => {
        activePastyId = id;

        await tick();

        createLineHighlightingEventHandlers();
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

    const createLineHighlightingEventHandlers = () => {
        if (!browser) return;

        const lineNumberElements = window.document.querySelectorAll(".line-number");

        let lastClickedLine: number | null = null;

        for (const lineNumberElement of lineNumberElements) {
            lineNumberElement.addEventListener("click", (el: Event) => {
                const currentTarget = el.currentTarget as HTMLElement;
                const line = parseInt(currentTarget.innerText, 10);
                const selectingRange = (el as MouseEvent).shiftKey && lastClickedLine !== null;
                const startLine = selectingRange ? Math.min(lastClickedLine!, line) : line;
                const endLine = selectingRange ? Math.max(lastClickedLine!, line) : line;

                const pastyIndex =
                    settings.pasteView === "stacked"
                        ? currentTarget
                              .closest(".pasty")!
                              .attributes.getNamedItem("data-pastyindex")!.value
                        : paste.pasties.findIndex((p) => p.id === activePastyId);

                const highlighted = window.document.querySelectorAll(".line.highlight");
                highlighted.forEach((hl) => {
                    hl.classList.remove("highlight", "first", "last");
                });

                for (let i = startLine; i <= endLine; i++) {
                    const element =
                        settings.pasteView === "stacked"
                            ? window.document.querySelector(
                                  `.pasty[data-pastyindex="${pastyIndex}"] .line[data-line="${i}"]`
                              )!
                            : window.document.querySelector(`.line[data-line="${i}"]`)!;

                    if (i === startLine) element.classList.add("highlight", "first");

                    if (i === endLine) element.classList.add("highlight", "last");

                    element.classList.add("highlight");
                }

                let hash = `${pastyIndex}L${startLine}`;
                if (selectingRange) {
                    hash += `-L${endLine}`;
                }

                window.location.hash = hash;

                lastClickedLine = line;
            });
        }
    };

    const highlightLines = async () => {
        if (!browser) return;

        const match = /(\d+)L(\d+)(?:-L(\d+))?/.exec(window.location.hash);
        if (match) {
            let [, pastyIndex, startLine, endLine] = match.map((m) => (m ? parseInt(m, 10) : null));

            if (!startLine) return;

            if (!endLine) {
                endLine = startLine;
            }

            await setActiveTab(paste.pasties[pastyIndex!].id);

            let firstElement: Element | null = null;

            for (let i = startLine; i <= endLine; i++) {
                const element =
                    settings.pasteView === "stacked"
                        ? window.document.querySelector(
                              `.pasty[data-pastyindex="${pastyIndex}"] .line[data-line="${i}"]`
                          )
                        : window.document.querySelector(`.line[data-line="${i}"]`);

                if (!firstElement) {
                    firstElement = element;
                }

                if (element) {
                    element.classList.add("highlight");
                    if (i === startLine) element.classList.add("first");
                    if (i === endLine) element.classList.add("last");
                }
            }

            if (firstElement) {
                const yOffset = -90;
                const y = firstElement.getBoundingClientRect().top + window.scrollY + yOffset;

                window.scrollTo({ top: y, behavior: "instant" });
            }
        }
    };

    onMount(() => {
        createLineHighlightingEventHandlers();
        highlightLines();
    });
</script>

<div class="pasties">
    {#if settings.pasteView === "stacked"}
        {#each paste.pasties as pasty, i}
            <div class="pasty" data-pastyindex={i}>
                <div class="sticky">
                    <div class="title flex row space-between center">
                        <span>{pasty.title || "untitled"}</span>

                        {#if pasteStats && pasteStats.pasties[pasty.id]}
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
                            onclick={async () => await setActiveTab(pasty.id)}
                        />
                    {/each}
                </div>
            </div>

            {#if pasteStats && pasteStats.pasties[activePastyId]}
                <div class="meta-tabbed">
                    <PastyMeta
                        {paste}
                        pasty={activePasty}
                        {langStats}
                        {historyId}
                        stats={pasteStats.pasties[activePastyId]}
                        bind:previewMarkdown={
                            previewMarkdown[paste.pasties.findIndex((p) => p.id === activePastyId)]
                        }
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
            min-width: max-content;
            width: auto;
        }

        :global(.shiki code .line-number) {
            margin-right: 0.5rem;
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
