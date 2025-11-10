<script lang="ts">
    import Tab from "./Tab.svelte";
    import Editor from "./Editor.svelte";
    import type { Settings } from "./api/settings";
    import TabData from "./TabData.svelte";
    import { nanoid } from "nanoid";
    import { onMount } from "svelte";
    import Sortable from "sortablejs";
    import { beforeNavigate } from "$app/navigation";
    import { creatingPasteStore } from "./stores.svelte";
    import type { Pasty } from "./api/paste";
    import { getLangs } from "./api/lang";

    interface Props {
        settings: Settings;
        existingPasties?: Pasty[];
    }

    let { settings, existingPasties = [] }: Props = $props();

    let tabs = $state<TabData[]>([new TabData(nanoid(), "untitled")]);

    // svelte-ignore state_referenced_locally
    let activeTabId = $state(tabs[0].id);

    let sortable: Sortable | null = $state(null);
    let tabGroupElement = $state<HTMLElement | null>(null);
    let editor = $state<Editor | null>(null);

    let isDraggedOver = $state(false);

    beforeNavigate((navigation) => {
        if ($creatingPasteStore) {
            $creatingPasteStore = false;
            return;
        }

        if (
            hasModifiedTabs() &&
            !confirm("you have modified content, are you sure you want to leave the current page?")
        ) {
            navigation.cancel();
        }
    });

    onMount(() => {
        if (!tabGroupElement) return;

        sortable = Sortable.create(tabGroupElement, {
            direction: "horizontal",
            animation: 150,
            delay: 50
        });
    });

    const onEditorMounted = async () => {
        if (existingPasties.length > 0) {
            tabs = [];
            const langs = await getLangs(fetch);
            for (const pasty of existingPasties) {
                const lang = langs.find((lang) => lang.name === pasty.language);
                const tab = new TabData(pasty.id, pasty.title);
                tab.content = pasty.content;
                tab.language = lang;

                tabs = [...tabs, tab];
            }

            setActiveTab(tabs[tabs.length - 1].id);
        }
    };

    const hasModifiedTabs = () => {
        if (!editor) return false;

        if (editor.getContent() && editor.getContent() !== "") return true;

        for (const tab of tabs) {
            if (tab.content !== "") return true;
        }

        return false;
    };

    const onNewTab = () => {
        const newTab = new TabData(nanoid(), "untitled");
        tabs = [...tabs, newTab];

        setActiveTab(newTab.id);
    };

    const onTabClose = (tabId: string) => {
        // cant close last tab
        if (tabs.length === 1) return;

        if (!editor) return;

        const idx = tabs.findIndex((t) => t.id === tabId);

        if ((tabId === activeTabId && editor.getContent() !== "") || tabs[idx].content !== "") {
            if (!confirm("are you sure you want to close a non-empty tab?")) {
                return;
            }
        }

        tabs = [...tabs.slice(0, idx), ...tabs.slice(idx + 1, tabs.length)];

        if (activeTabId === tabId) {
            // if the closed tab was active, set the previous tab as active
            setActiveTab(tabs[idx - 1]?.id ?? tabs[0].id);
        }
    };

    const onTabClick = (event: Event & { currentTarget: EventTarget & Element }, tabId: string) => {
        let target = event.target as HTMLElement;

        // ignore if renaming
        const tab = tabs.find((t) => t.id === tabId);
        if (tab?.isInRenamingState) return;

        // ignore if close icon is pressed
        if (target.nodeName === "BUTTON") return;

        // ignore if double click, so the rename field doesn't unfocus
        if ((event as unknown as MouseEvent).detail > 1) return;

        setActiveTab(tabId);
    };

    const setActiveTab = (tabId: string) => {
        if (!editor) return;

        const previousTabIdx = tabs.findIndex((t) => t.id === activeTabId);
        if (activeTabId && previousTabIdx !== -1) {
            const cursor = editor.getCursorPos();
            const indentation = editor.getIndentation();
            tabs[previousTabIdx].content = editor.getContent();
            tabs[previousTabIdx].cursorLine = cursor.line;
            tabs[previousTabIdx].cursorCol = cursor.col;
            tabs[previousTabIdx].language = editor.getSelectedLang();
            tabs[previousTabIdx].indentationUnit = indentation[0];
            tabs[previousTabIdx].indentationWidth = indentation[1];
        }

        activeTabId = tabId;

        const idx = tabs.findIndex((t) => t.id === tabId);
        editor.setContent(tabs[idx].content);
        editor.setCursorPos(tabs[idx].cursorLine, tabs[idx].cursorCol);
        if (tabs[idx].language) editor.setSelectedLang(tabs[idx].language);
        editor.setIndentaion(tabs[idx].indentationUnit, tabs[idx].indentationWidth);

        editor.focus();
    };

    const handleDragOver = (e: DragEvent) => {
        e.preventDefault();

        if (!e.dataTransfer?.types.includes("Files")) return;

        isDraggedOver = true;
    };

    const handleDragDrop = async (e: DragEvent) => {
        e.preventDefault();

        isDraggedOver = false;

        const files = e.dataTransfer?.files;

        if (!files || files.length === 0) return;

        const langs = await getLangs(fetch);

        if (tabs[0].content === "") {
            tabs = [];
        }

        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const { name } = file;

            const content = await file.text();

            const lang = langs.find(
                (l) => l.extensions && l.extensions.includes(name.slice(name.lastIndexOf(".")))
            );

            const tab = new TabData(nanoid(), name);
            tab.content = content;
            tab.language = lang;

            tabs = [...tabs, tab];
        }

        setActiveTab(tabs[tabs.length - 1].id);
    };

    const handleDragLeave = () => {
        isDraggedOver = false;
    };

    const onTabFinishedRenaming = async (tab: TabData) => {
        const langs = await getLangs(fetch);

        const lang = langs.find(
            (l) =>
                l.extensions && l.extensions.includes(tab.title.slice(tab.title.lastIndexOf(".")))
        );

        if (lang) {
            tab.language = lang;
            editor?.setSelectedLang(tab.language);
        }

        editor?.focus();
    };

    export const getTabs = () => {
        if (!editor) return tabs;

        // save the current editor state to the current tab
        const idx = tabs.findIndex((t) => t.id === activeTabId);
        const cursor = editor.getCursorPos();
        const indentation = editor.getIndentation();
        tabs[idx].content = editor.getContent();
        tabs[idx].cursorLine = cursor.line;
        tabs[idx].cursorCol = cursor.col;
        tabs[idx].language = editor.getSelectedLang();
        tabs[idx].indentationUnit = indentation[0];
        tabs[idx].indentationWidth = indentation[1];

        if (!sortable) return [];

        // sort tabs based on their current order in the DOM
        const sortedTabs = sortable.toArray().map((id) => {
            return tabs.find((tab) => tab.id === id)!;
        });

        return sortedTabs;
    };

    export const getLanguageCommands = () => {
        if (!editor) return [];

        return editor.getLanguageCommands();
    };

    export const getIndentUnitCommands = (convertIndent = false) => {
        if (!editor) return [];

        return editor.getIndentUnitCommands(convertIndent);
    };

    export const getSelectedLang = () => {
        if (!editor) return undefined;

        return editor.getSelectedLang();
    };

    export const getSelectedIndentationUnit = () => {
        if (!editor) return undefined;

        return editor.getIndentation()[0];
    };
</script>

<svelte:window
    onbeforeunload={(e) => {
        if (hasModifiedTabs()) {
            e.preventDefault();
            e.returnValue = "";
            return "";
        }
    }}
/>

<svelte:body ondragover={handleDragOver} ondrop={handleDragDrop} ondragleave={handleDragLeave} />

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
                onclick={(event) => onTabClick(event, tab.id)}
                onfinishedRenaming={() => onTabFinishedRenaming(tab)}
                bind:title={tab.title}
                bind:isInRenamingState={tab.isInRenamingState}
                isActive={tab.id === activeTabId}
                closeable={tabs.length > 1}
                isReadonly={false}
            />
        {/each}
    </div>

    <button class="add-btn btn" onclick={() => onNewTab()}>
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

<Editor bind:this={editor} {settings} onMounted={onEditorMounted} />

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
