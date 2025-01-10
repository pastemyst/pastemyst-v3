<script lang="ts">
    import { onMount, tick, mount, unmount } from "svelte";
    import Sortable, { type SortableEvent } from "sortablejs";
    import Tab from "./Tab.svelte";
    import Editor from "./Editor.svelte";
    import TabData from "./TabData.svelte";
    import { getLangs, type Language } from "./api/lang";
    import { beforeNavigate } from "$app/navigation";
    import { creatingPasteStore } from "./stores";
    import type { Settings } from "./api/settings";
    import type { Pasty } from "./api/paste";

    interface Props {
        settings: Settings;
        tabs: TabData[];
        activeTab?: TabData;
        existingPasties?: Pasty[];
    }

    let {
        settings,
        tabs = $bindable([]),
        activeTab = $bindable(),
        existingPasties = []
    }: Props = $props();

    // used for giving tabs their own unique ID
    let tabCounter = 0;

    let tabGroupElement: HTMLElement;
    let editorTarget: HTMLElement;

    let activeTabId = $state("0");

    let isDraggedOver = $state(false);

    $effect(() => {
        activeTab = tabs.find((t) => t.id === activeTabId)!;
    });

    beforeNavigate((navigation) => {
        if (creatingPasteStore) {
            creatingPasteStore.set(false);
            return;
        }

        if (
            hasModifiedTabs() &&
            !confirm("you have modified content, are you sure you want to leave the current page?")
        ) {
            navigation.cancel();
        }
    });

    onMount(async () => {
        Sortable.create(tabGroupElement, {
            direction: "horizontal",
            animation: 150,
            delay: 50,

            onEnd: (event: SortableEvent) => {
                // once the reordering of tabs is done, replicate the reorder in the data array
                if (event.oldIndex && event.newIndex) {
                    const tab = tabs[event.oldIndex];
                    tabs.splice(event.oldIndex, 1);
                    tabs.splice(event.newIndex, 0, tab);
                    tabs = tabs;
                }
            }
        });

        if (existingPasties.length > 0) {
            const langs = await getLangs(fetch);
            for (const pasty of existingPasties) {
                const lang = langs.find((lang) => lang.name === pasty.language);
                await addTab(pasty.title, pasty.content, lang, pasty.id);
            }

            await setActiveTab(tabs[0].id);
        } else {
            await addTab();
        }
    });

    const onTabClose = async (id: string) => {
        // cant close last tab
        if (tabs.length === 1) return;

        const idx = tabs.findIndex((t) => t.id === id);

        // ask if it's okay to close a non empty tab
        if (tabs[idx].editor!.getContent().length > 0) {
            if (!confirm("are you sure you want to close a non-empty tab?")) {
                return;
            }
        }

        // destroy editor element
        unmount(tabs[idx].editor!);

        // remove from array
        tabs = [...tabs.slice(0, idx), ...tabs.slice(idx + 1, tabs.length)];

        if (tabs.findIndex((t) => t.id === activeTabId) >= 1) {
            // set the previous tab as active
            await setActiveTab(tabs[tabs.length - 1].id);
        } else {
            // if the first tab is closed, set the next tab as active
            await setActiveTab(tabs[0].id);
        }

        updateTabEditorVisibility();
    };

    const onTabClick = async (
        id: string,
        event: Event & { currentTarget: EventTarget & Element }
    ) => {
        let target = event.target as HTMLElement;

        // ignore if close icon is pressed
        if (target.nodeName === "SVG") return;

        // ignore if double click, so the rename field doesn't unfocus
        if ((event as unknown as MouseEvent).detail > 1) return;

        await setActiveTab(id);
    };

    const onTabFinishRenaming = (id: string) => {
        const idx = tabs.findIndex((t) => t.id === id);

        tabs[idx].editor!.focus();
    };

    export const addTab = async (
        title?: string,
        content?: string,
        language?: Language,
        id?: string
    ) => {
        const name = title || "untitled";

        let newtab: TabData = new TabData(
            id || String(tabCounter),
            name,
            mount(Editor, {
                target: editorTarget,
                props: {
                    settings,
                    onMounted: () => {
                        if (content && language) {
                            newtab.editor?.setContent(content);
                            newtab.editor?.setSelectedLang(language);

                            // if the first tab is empty, remove it
                            const firstTab = tabs[0];
                            if (
                                tabs.length > 1 &&
                                !firstTab.editor?.getContent() &&
                                firstTab.title === "untitled"
                            ) {
                                tabs = tabs.slice(1);
                            }
                        } else {
                            copyPreviousTabSettings(newtab.editor!);
                        }
                    }
                }
            })
        );

        tabs = [...tabs, newtab];
        await setActiveTab(tabs[tabs.length - 1].id);

        tabCounter++;
    };

    // set the language and indentation same as the previous tab
    const copyPreviousTabSettings = (newTabEditor: ReturnType<typeof Editor>) => {
        if (tabs.length > 1) {
            const previousEditor = tabs[tabs.length - 2].editor;

            newTabEditor.setSelectedLang(previousEditor!.getSelectedLang()!);
            newTabEditor.setIndentaion(...previousEditor!.getIndentation());
        }
    };

    const setActiveTab = async (id: string) => {
        activeTabId = id;

        updateTabEditorVisibility();

        // for some reason requires 2 ticks to pass for the editor to be focused correctly
        await tick();
        await tick();

        let tab = tabs.find((t) => t.id === activeTabId);

        if (tab && !tab.isInRenamingState) {
            tab.editor!.focus();
        }
    };

    const updateTabEditorVisibility = () => {
        for (let tab of tabs) {
            tab.editor?.setHidden(!(tab.id === activeTabId));
        }
    };

    const closeDragContainer = () => (isDraggedOver = false);

    const handleDragOver = (e: DragEvent) => {
        e.preventDefault();
        if (!e.dataTransfer?.types.includes("Files")) return;
        isDraggedOver = true;
    };

    const handleDragDrop = (e: DragEvent) => {
        e.preventDefault();

        closeDragContainer();

        const files = e.dataTransfer?.files;

        if (!files) return;

        Array.from(files).forEach(async (file) => {
            const { name } = file;

            const content = await file.text();

            const langs = await getLangs(fetch);
            const lang = langs.find(
                (lang) =>
                    lang.extensions && lang.extensions.includes(name.slice(name.lastIndexOf(".")))
            );

            await addTab(name, content, lang);
        });
    };

    const hasModifiedTabs = () => {
        for (const tab of tabs) {
            if (tab.editor?.getContent()) return true;
        }

        return false;
    };
</script>

<svelte:window
    onbeforeunload={(e) => {
        if (hasModifiedTabs()) {
            // prevent default only after the check, firefox issue
            e.preventDefault();

            // next 2 lines are needed to work
            e.returnValue = "";
            return "";
        }
    }}
/>

<svelte:body ondragover={handleDragOver} ondrop={handleDragDrop} ondragleave={closeDragContainer} />

<div class="drop-container" class:drop-container--shown={isDraggedOver}>
    <div class="drop-container__cover"></div>
    <p>drop files here</p>
</div>

<div class="tabs flex row center">
    <div class="tabgroup flex row" bind:this={tabGroupElement}>
        <!-- eslint-disable-next-line @typescript-eslint/no-unused-vars -->
        {#each tabs as tab, _ (tab.id)}
            <Tab
                id={tab.id.toString()}
                onclose={() => onTabClose(tab.id)}
                onclick={(event) => onTabClick(tab.id, event)}
                onfinishedRenaming={() => onTabFinishRenaming(tab.id)}
                bind:title={tab.title}
                bind:isInRenamingState={tab.isInRenamingState}
                isActive={activeTabId === tab.id}
                closeable={tabs.length > 1}
                isReadonly={false}
            />
        {/each}
    </div>

    <button class="add-btn btn" onclick={() => addTab()}>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
            <title>Plus Icon</title>
            <path
                fill="currentColor"
                fill-rule="evenodd"
                d="M7.75 2a.75.75 0 01.75.75V7h4.25a.75.75 0 110 1.5H8.5v4.25a.75.75 0 11-1.5 0V8.5H2.75a.75.75 0 010-1.5H7V2.75A.75.75 0 017.75 2z"
            />
        </svg>
    </button>
</div>

<div class="editor" bind:this={editorTarget}></div>

<style lang="scss">
    .drop-container {
        position: fixed;
        inset: 0;
        z-index: 105;
        display: flex;
        justify-content: center;
        align-items: center;
        font-size: 7rem;
        display: none;
        pointer-events: none;

        &--shown {
            display: flex;
        }

        &__cover {
            position: absolute;
            pointer-events: none;
            inset: 0;
            opacity: 0.3;
            background-color: var(--color-bg3);
        }
    }

    .tabs {
        width: 100%;
        box-sizing: border-box;
        background-color: var(--color-bg2);
        border-radius: $border-radius $border-radius 0 0;
        margin-bottom: -1px; // collapse bottom border

        .tabgroup {
            flex-wrap: wrap;
        }

        .add-btn {
            margin: 0 0.5rem;
            border-bottom-color: transparent;
            background-color: transparent;

            &:focus,
            &:active {
                border-bottom-color: var(--color-primary) !important;
            }

            &:hover {
                border-bottom-color: var(--color-bg3);
            }

            .icon {
                max-width: 15px;
                max-height: 15px;
            }
        }
    }

    .tabs .tabgroup :global(.sortable-drag) {
        opacity: 0;
    }

    .tabs .tabgroup :global(.sortable-chosen) {
        @include shadow-big();
        z-index: 10;
        border-color: var(--color-secondary);
    }
</style>
