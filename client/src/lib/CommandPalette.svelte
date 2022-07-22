<script lang="ts">
    import highlightWords, { type HighlightWords } from "highlight-words";
    import { tick } from "svelte";
    import {
        Command,
        DirCommand,
        rootCommands,
        SelectCommand,
        SelectOptionCommand,
        LinkCommand
    } from "./cmdOptions";
    import { isCommandPaletteOpen } from "./stores";
    export let isOpen: boolean = false;

    let searchElement: HTMLInputElement;
    let search: string;
    let searchFound: boolean = true;

    let isCommandSelected: boolean = false;

    let commandsElement: HTMLElement;

    let commands: Command[] = rootCommands;
    let filteredCommands: Command[] = commands;
    let highlightedChunks: HighlightWords.Chunk[][][];

    let elements: HTMLElement[] = [];
    let selectedCommand: Command;

    let lastActiveElement: Element | null;

    const onCmd = async (e: MouseEvent | null, cmd: Command) => {
        if (cmd instanceof DirCommand) {
            e?.preventDefault();

            commands = cmd.subCommands;
            filteredCommands = commands;
            selectedCommand = commands[0];
            search = "";
            searchElement.focus();

            await tick();

            scrollSelectedIntoView();
        } else if (cmd instanceof SelectCommand) {
            e?.preventDefault();

            commands = cmd.options;
            filteredCommands = commands;
            selectedCommand = commands.find((c) => (c as SelectOptionCommand).selected)!;
            search = "";
            searchElement.focus();

            await tick();

            scrollSelectedIntoView();
        } else if (cmd instanceof SelectOptionCommand) {
            e?.preventDefault();

            for (let opt of cmd.parent.options) {
                opt.selected = false;
            }

            cmd.selected = true;

            setOpen(false);
        }
    };

    export const showOptions = (cmd: Command) => {
        setOpen(true);

        onCmd(null, cmd);
    };

    const setOpen = async (v: boolean, updateStore = true) => {
        if (v) {
            lastActiveElement = document.activeElement;

            selectedCommand = filteredCommands[0];

            searchElement?.focus();
            search = "";

            await tick();

            scrollSelectedIntoView();
        } else {
            commands = rootCommands;
            filteredCommands = commands;

            (lastActiveElement as HTMLElement)?.focus();
        }

        if (updateStore) {
            isCommandPaletteOpen.set(v);
        }

        isOpen = v;
    };

    const onSearchBlur = async () => {
        if (isCommandSelected) return;

        await setOpen(false);
    };

    const onCommandMouseDown = (cmd: Command) => {
        isCommandSelected = true;
        selectedCommand = cmd;
    };

    const onCommandMouseUp = () => {
        isCommandSelected = false;
    };

    const onKeyDown = async (e: KeyboardEvent) => {
        switch (e.key) {
            case "Enter":
                {
                    e.preventDefault();
                    const index = filteredCommands.findIndex((e) => e === selectedCommand);
                    elements[index].click();
                }
                break;

            case "ArrowUp":
                {
                    e.preventDefault();
                    const index = filteredCommands.findIndex((e) => e === selectedCommand);
                    let newIndex = index - 1;
                    if (newIndex < 0) newIndex = filteredCommands.length - 1;
                    selectedCommand = filteredCommands[newIndex];

                    scrollSelectedIntoView();
                }
                break;

            case "ArrowDown":
                {
                    e.preventDefault();
                    const index = filteredCommands.findIndex((e) => e === selectedCommand);
                    const newIndex = (index + 1) % filteredCommands.length;
                    selectedCommand = filteredCommands[newIndex];

                    scrollSelectedIntoView();
                }
                break;

            case "Escape":
                {
                    e.preventDefault();
                    await setOpen(false);
                }
                break;
        }
    };

    const scrollSelectedIntoView = () => {
        const index = filteredCommands.findIndex((e) => e === selectedCommand);
        commandsElement.scrollTop = elements[index].offsetTop - 100 - commandsElement.offsetTop;
    };

    const filter = () => {
        filteredCommands = commands.filter(
            (e) =>
                e.name.toLowerCase().indexOf(search.toLowerCase()) > -1 ||
                e.description?.toLowerCase().indexOf(search.toLowerCase())! > -1
        );
        searchFound = filteredCommands.length > 0;

        if (searchFound) {
            selectedCommand = filteredCommands[0];
        }

        highlightedChunks = [];

        for (const cmd of filteredCommands) {
            const chunks = [];

            chunks.push(
                highlightWords({
                    text: cmd.name,
                    query: search,
                    matchExactly: true
                })
            );

            if (cmd.description) {
                chunks.push(
                    highlightWords({
                        text: cmd.description,
                        query: search,
                        matchExactly: true
                    })
                );
            }

            highlightedChunks.push(chunks);
        }
    };
</script>

<svelte:window
    on:cmdShowOptions={(e) => showOptions(e.detail)}
    on:openCmd={() => setOpen(true)}
    on:toggleCmd={() => setOpen(!isOpen)}
/>

<div role="dialog" aria-modal="true" class="palette" class:isOpen>
    <div class="wrapper">
        <div class="search flex row center">
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M18.319 14.4326C20.7628 11.2941 20.542 6.75347 17.6569 3.86829C14.5327 0.744098 9.46734 0.744098 6.34315 3.86829C3.21895 6.99249 3.21895 12.0578 6.34315 15.182C9.22833 18.0672 13.769 18.2879 16.9075 15.8442C16.921 15.8595 16.9351 15.8745 16.9497 15.8891L21.1924 20.1317C21.5829 20.5223 22.2161 20.5223 22.6066 20.1317C22.9971 19.7412 22.9971 19.1081 22.6066 18.7175L18.364 14.4749C18.3493 14.4603 18.3343 14.4462 18.319 14.4326ZM16.2426 5.28251C18.5858 7.62565 18.5858 11.4246 16.2426 13.7678C13.8995 16.1109 10.1005 16.1109 7.75736 13.7678C5.41421 11.4246 5.41421 7.62565 7.75736 5.28251C10.1005 2.93936 13.8995 2.93936 16.2426 5.28251Z"
                    fill="currentColor"
                />
            </svg>

            <!-- search -->
            <input
                type="text"
                name="search"
                placeholder="search..."
                spellcheck="false"
                autocomplete="off"
                bind:this={searchElement}
                on:blur={onSearchBlur}
                on:keydown={onKeyDown}
                bind:value={search}
                on:input={filter}
            />
        </div>

        <!-- list of commands -->
        <div class="commands" bind:this={commandsElement}>
            {#each filteredCommands as cmd, cmdIndex}
                <a
                    href={cmd instanceof LinkCommand ? cmd.url : null}
                    class="command flex col no-dec"
                    target="_blank"
                    on:click={(e) => onCmd(e, cmd)}
                    on:mousedown={() => onCommandMouseDown(cmd)}
                    on:mouseup={onCommandMouseUp}
                    class:selected={selectedCommand === cmd}
                    class:selected-option={cmd instanceof SelectOptionCommand && cmd.selected}
                    bind:this={elements[cmdIndex]}
                >
                    <div class="name flex row center">
                        <!-- icon -->
                        {#if cmd.icon}
                            <div class="icon flex row center">
                                {@html cmd.icon}
                            </div>
                        {:else if cmd instanceof SelectOptionCommand && cmd.selected}
                            <div class="icon flex row center">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    class="ionicon"
                                    viewBox="0 0 512 512"
                                >
                                    <title>Checkmark</title>
                                    <path
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                        stroke-width="32"
                                        d="M416 128L192 384l-96-96"
                                    />
                                </svg>
                            </div>
                        {/if}

                        <!-- name -->
                        {#if search !== undefined && search !== ""}
                            {#each highlightedChunks[cmdIndex][0] as chunk (chunk.key)}
                                <span aria-hidden="true" class:highlight={chunk.match}>
                                    {chunk.text}
                                </span>
                            {/each}
                        {:else}
                            {cmd.name}
                        {/if}

                        <!-- shortcuts -->
                        {#if cmd.shortcuts}
                            <div class="shortcuts flex row">
                                {#each cmd.shortcuts as shortcuts}
                                    <div class="shortcut flex row center">
                                        {#each shortcuts as key, index}
                                            <kbd>{key}</kbd>
                                            {#if index < shortcuts.length - 1}
                                                <span>+</span>
                                            {/if}
                                        {/each}
                                    </div>
                                {/each}
                            </div>
                        {/if}
                    </div>

                    <!-- description -->
                    {#if cmd.description}
                        <div class="description">
                            {#if search !== undefined && search !== ""}
                                {#each highlightedChunks[cmdIndex][1] as chunk (chunk.key)}
                                    <span aria-hidden="true" class:highlight={chunk.match}>
                                        {chunk.text}
                                    </span>
                                {/each}
                            {:else}
                                {cmd.description}
                            {/if}
                        </div>
                    {/if}
                </a>
            {/each}
        </div>
    </div>
</div>

<style lang="scss">
    .palette {
        @include transition(opacity);
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        font-size: $fs-normal;
        opacity: 0;
        height: 0;
        overflow: hidden;
        z-index: 9999;

        &.isOpen {
            opacity: 1;
            height: 100vh;
        }
    }

    .wrapper {
        background-color: $color-bg;
        width: 40rem;
        margin: 0 auto;
        border-radius: $border-radius;
        border-top-left-radius: 0;
        border-top-right-radius: 0;
        border: 1px solid $color-bg-2;
        border-top: 0;
        @include shadow-big();
    }

    .search {
        padding: 0.5rem;
        border-bottom: 1px solid $color-bg-1;

        .icon {
            margin-right: 1rem;
            font-size: $fs-medium;
        }

        input {
            background: transparent;
            border: none;
            outline: none;
            color: $color-fg;
            width: 100%;
            padding: 0;
        }
    }

    .commands {
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
        overflow-y: auto;
        max-height: 275px;

        .command {
            color: $color-fg;
            padding: 0.5rem;
            cursor: pointer;

            &.selected,
            &:hover {
                background-color: $color-bg-1;
                border-left: 2px solid $color-sec !important;
                padding-left: 0.75rem;
            }

            &.selected-option {
                border-left: 2px solid $color-prim;
                padding-left: 0.75rem;
            }

            .highlight {
                color: $color-sec;
            }

            .name {
                .icon {
                    margin-right: 0.5rem;
                    color: $color-fg;
                    max-width: 20px;
                }
            }

            .description {
                color: $color-bg-3;
                font-size: $fs-small;

                span {
                    display: inline-block;
                }

                .highlight {
                    display: inline-block;
                }
            }

            .shortcuts {
                margin-left: auto;

                .shortcut {
                    margin-left: 1rem;

                    kbd {
                        font-size: 0.8rem;
                    }

                    span {
                        margin-left: 0.3rem;
                        margin-right: 0.3rem;
                        margin-top: -2px;
                    }
                }
            }
        }
    }
</style>
