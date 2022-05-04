<script lang="ts">
    import { onMount, tick } from "svelte";
    import Sortable, { SortableEvent } from "sortablejs";
    import Tab from "./Tab.svelte";
    import Editor from "./Editor.svelte";
    import TabData from "./TabData";

    export let tabs: TabData[] = new Array<TabData>();

    // used for giving tabs their own unique ID
    let tabCounter = 0;

    let tabGroupElement: HTMLElement;
    let editorTarget: HTMLElement;

    let activeTabId: number = 0;

    onMount(async () => {
        Sortable.create(tabGroupElement, {
            direction: "horizontal",
            animation: 150,
            delay: 50,

            onEnd: (event: SortableEvent) => {
                // once the reordering of tabs is done, replicate the reorder in the data array
                const tab = tabs[event.oldIndex];
                tabs.splice(event.oldIndex, 1);
                tabs.splice(event.newIndex, 0, tab);
                tabs = tabs;
            }
        });

        await addTab();
    });

    const onTabClose = async (id: number) => {
        // cant close last tab
        if (tabs.length === 1) return;

        const idx = tabs.findIndex((t) => t.id === id);

        // destroy editor element
        tabs[idx].editor.$destroy();

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

    const onTabClick = async (id: number, event: CustomEvent<any>) => {
        let target = event.detail.event.target as HTMLElement;

        // ignore if close icon is pressed
        if (target.nodeName === "SVG") return;

        // ignore if double click, so the rename field doesn't unfocus
        if (event.detail.event.detail > 1) return;

        await setActiveTab(id);
    };

    const addTab = async () => {
        const name = tabs.length > 0 ? `untitled ${tabCounter}` : "untitled";

        let newtab = new TabData(tabCounter, name, new Editor({ target: editorTarget }));

        await tick();

        tabs = [...tabs, newtab];
        await setActiveTab(tabs[tabs.length - 1].id);

        tabCounter++;
    };

    const setActiveTab = async (id: number) => {
        activeTabId = id;

        updateTabEditorVisibility();

        // for some reason requires 2 ticks to pass for the editor to be focused correctly
        await tick();
        await tick();

        let tab = tabs.find((t) => t.id === activeTabId);

        if (!tab.isInRenamingState) {
            tab.editor.focus();
        }
    };

    const updateTabEditorVisibility = () => {
        for (let tab of tabs) {
            tab.editor.$set({ hidden: !(tab.id === activeTabId) });
        }
    };
</script>

<div class="tabs flex row center">
    <div class="tabgroup flex row" bind:this={tabGroupElement}>
        {#each tabs as tab, i (tab.id)}
            <Tab
                id={tab.id.toString()}
                on:close={() => onTabClose(tab.id)}
                on:click={(event) => onTabClick(tab.id, event)}
                bind:isInRenamingState={tab.isInRenamingState}
                bind:title={tab.title}
                isActive={activeTabId === tab.id}
                closeable={tabs.length > 1}
            />
        {/each}
    </div>

    <div class="add-btn btn" on:click={addTab}>
        <svg
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            class="icon"
        >
            <path
                d="M12 4C11.4477 4 11 4.44772 11 5V11H5C4.44772 11 4 11.4477 4 12C4 12.5523 4.44772 13 5 13H11V19C11 19.5523 11.4477 20 12 20C12.5523 20 13 19.5523 13 19V13H19C19.5523 13 20 12.5523 20 12C20 11.4477 19.5523 11 19 11H13V5C13 4.44772 12.5523 4 12 4Z"
                fill="currentColor"
            />
        </svg>
    </div>
</div>

<div class="editor" bind:this={editorTarget} />

<style lang="scss">
    .tabs {
        width: 100%;
        box-sizing: border-box;
        background-color: $color-bg-2;
        border-radius: $border-radius $border-radius 0 0;
        margin-bottom: -1px; // collapse bottom border

        .tabgroup {
            flex-wrap: wrap;
        }

        .add-btn {
            margin: 0 0.5rem;
            border-bottom-color: transparent;
            background-color: transparent;

            &:hover {
                border-bottom-color: $color-bg-3;
            }

            .icon {
                max-width: 15px;
                max-height: 15px;
            }
        }
    }

    .tabs .tabgroup :global(.sortable-drag) {
        display: none;
    }

    .tabs .tabgroup :global(.sortable-chosen) {
        @include shadow-big();
        z-index: 10;
        border-color: $color-sec;
    }
</style>
