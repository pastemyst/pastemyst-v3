<script lang="ts">
    import type { HighlightWords } from "highlight-words";
    import highlightWords from "highlight-words";
    import { onMount } from "svelte";
    import type { Command } from "./command";
    import { cmdPalCommands, cmdPalOpen } from "./stores";

    let isOpen = false;

    // save the current commands, next time the palette is open, switch to the previous commands
    // used for modal dialogs
    let prevCommands: Command[] | undefined = undefined;

    let commands: Command[] = [];
    let filteredCommands: Command[] = [];

    let searchElement: HTMLInputElement | undefined;
    let search = "";

    let highlightedChunks: HighlightWords.Chunk[][][];

    let isCommandSelected = false;
    let selectedCommand: Command | undefined;
    let commandElements: HTMLElement[] = [];

    // keep track of the last focused element, so we can focus it back once the command palette is closed
    let lastFocusedElement: Element | null;

    onMount(() => {
        cmdPalOpen.subscribe((val) => {
            if (val) {
                open();
            } else {
                close();
            }
        });

        cmdPalCommands.subscribe((cmds) => {
            prevCommands = commands;

            commands = cmds;
            filteredCommands = commands;
        });
    });

    const handleKeys = (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key === "k") {
            e.preventDefault();
            toggle();
        }
    };

    const onSearchBlur = () => {
        if (isCommandSelected) return;

        close();
    };

    const onSearchKeyDown = (e: KeyboardEvent) => {
        switch (e.key) {
            case "Escape":
                {
                    e.preventDefault();
                    close();
                }
                break;

            case "Enter":
                {
                    e.preventDefault();

                    selectedCommand?.action();
                    close();
                }
                break;

            case "ArrowUp":
                {
                    e.preventDefault();

                    const index = filteredCommands.findIndex((cmd) => cmd === selectedCommand);
                    let newIndex = index - 1;
                    if (newIndex < 0) newIndex = filteredCommands.length - 1;

                    selectedCommand = filteredCommands[newIndex];

                    scrollIntoView();
                }
                break;

            case "ArrowDown":
                {
                    e.preventDefault();

                    const index = filteredCommands.findIndex((cmd) => cmd === selectedCommand);
                    const newIndex = (index + 1) % filteredCommands.length;

                    selectedCommand = filteredCommands[newIndex];

                    scrollIntoView();
                }
                break;
        }
    };

    const scrollIntoView = () => {
        const index = filteredCommands.findIndex((cmd) => cmd === selectedCommand);

        if (index === -1) return;

        commandElements[index]?.scrollIntoView({
            behavior: "smooth",
            block: "nearest"
        });
    };

    const filter = () => {
        filteredCommands = commands.filter((cmd) => {
            // by name
            if (cmd.name.toLowerCase().indexOf(search.toLowerCase()) > -1) {
                return cmd;
            }

            // by description
            if (
                cmd.description &&
                cmd.description.toLowerCase().indexOf(search.toLowerCase()) > -1
            ) {
                return cmd;
            }
        });

        if (filteredCommands.length > 0) {
            // make sure exact matches are on top
            filteredCommands = filteredCommands.sort((a, b) => {
                const a1: number = a.name.toLowerCase() === search.toLowerCase() ? -1 : 1;
                const b1: number = b.name.toLowerCase() === search.toLowerCase() ? -1 : 1;

                return Math.min(Math.max(a1 - b1, -1), 1);
            });

            selectedCommand = filteredCommands[0];
        } else {
            // deselect commands
            isCommandSelected = false;
            selectedCommand = undefined;
        }

        highlightedChunks = [];
        for (const cmd of filteredCommands) {
            const chunks = [];

            // highlight name
            chunks.push(
                highlightWords({
                    text: cmd.name,
                    query: search,
                    matchExactly: true
                })
            );

            // highlight description
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

    const toggle = () => {
        if (isOpen) {
            close();
        } else {
            open();
        }
    };

    const open = () => {
        isOpen = true;

        // save the focused element
        lastFocusedElement = document.activeElement;

        searchElement?.focus();

        selectedCommand = commands[0];

        cmdPalOpen.set(true);

        scrollIntoView();
    };

    const close = () => {
        isOpen = false;

        search = "";
        filteredCommands = commands;
        highlightedChunks = [];

        cmdPalOpen.set(false);

        // if previous commands are defined, switch to those
        if (prevCommands !== undefined && prevCommands.length > 0) {
            commands = prevCommands;
            filteredCommands = commands;

            prevCommands = undefined;
        }

        // restore focus
        (lastFocusedElement as HTMLElement)?.focus();
    };

    const onCmd = (cmd: Command) => {
        cmd.action();

        close();
    };

    const onCmdMouseDown = (cmd: Command) => {
        selectedCommand = cmd;
        isCommandSelected = true;
    };

    const onCmdMouseUp = () => {
        selectedCommand = undefined;
        isCommandSelected = false;
    };
</script>

<svelte:window on:keydown={handleKeys} />

<div role="dialog" aria-modal="true" class="palette-bg" class:open={isOpen}>
    <div class="palette">
        <div class="search flex row center">
            <input
                type="text"
                name="search"
                placeholder="search..."
                spellcheck="false"
                autocomplete="off"
                bind:this={searchElement}
                bind:value={search}
                on:blur={onSearchBlur}
                on:keydown={onSearchKeyDown}
                on:input={filter}
            />
        </div>

        <div class="commands">
            {#if commands.length === 0}
                <p class="no-commands">there aren't any commands defined.</p>
            {/if}

            {#if filteredCommands.length === 0}
                <p class="no-commands">no matching commands</p>
            {/if}

            {#each filteredCommands as cmd, i}
                <button
                    class="command flex col"
                    on:click={() => onCmd(cmd)}
                    on:mousedown={() => onCmdMouseDown(cmd)}
                    on:mouseup={onCmdMouseUp}
                    bind:this={commandElements[i]}
                    class:selected={selectedCommand === cmd}
                >
                    <div class="name">
                        {#if search && search !== "" && highlightedChunks[0]}
                            {#each highlightedChunks[i][0] as chunk (chunk.key)}
                                <span aria-hidden="true" class:highlight={chunk.match}>{chunk.text}</span>
                            {/each}
                        {:else}
                            <p>{cmd.name}</p>
                        {/if}
                    </div>

                    {#if cmd.description}
                        <div class="description">
                            {#if search && search !== "" && highlightedChunks}
                                {#each highlightedChunks[i][1] as chunk (chunk.key)}
                                    <span aria-hidden="true" class:highlight={chunk.match}>{chunk.text}</span>
                                {/each}
                            {:else}
                                <p>{cmd.description}</p>
                            {/if}
                        </div>
                    {/if}
                </button>
            {/each}
        </div>
    </div>
</div>

<style lang="scss">
    .palette-bg {
        position: absolute;
        top: 0;
        left: 50%;
        transform: translateX(-50%);
        width: 90%;
        font-size: $fs-normal;
        overflow: hidden;
        z-index: 999;
        opacity: 0;
        height: 0;
        @include transition(opacity);

        &.open {
            opacity: 1;
            height: 100vh;
        }
    }

    .palette {
        background-color: $color-bg;
        max-width: 40rem;
        margin: 0 auto;
        border-radius: $border-radius;
        border: 1px solid $color-bg-2;
        position: relative;
        top: 25%;
        max-height: 20rem;
        @include shadow-big();
    }

    .search {
        border-bottom: 1px solid $color-bg-2;

        input {
            background-color: $color-bg;
            border: none;
            width: 100%;
        }
    }

    .commands {
        max-height: 275px;
        overflow-y: auto;

        .no-commands {
            margin: 0;
            padding: 0.5rem;
        }

        .command {
            padding: 0.5rem;
            cursor: pointer;
            background-color: transparent;
            border: none;
            width: 100%;
            border-radius: 0;
            white-space: pre;
            align-items: flex-start;

            &:hover {
                background-color: $color-bg-1;
            }

            &.selected {
                background-color: $color-bg-2;
            }

            p {
                margin: 0;
            }

            .description {
                color: $color-bg-3;
                font-size: $fs-small;
            }

            .highlight {
                color: $color-sec;
            }
        }
    }
</style>
