<script lang="ts">
    import { onMount } from "svelte";
    import type { Command } from "./command";
    import { cmdPalCommands, cmdPalOpen } from "./stores";

    let isOpen = false;

    let commands: Command[] = [];
    let filteredCommands: Command[] = [];

    let searchElement: HTMLInputElement | undefined;
    let search = "";

    let isCommandSelected = false;
    let selectedCommand: Command | undefined;

    onMount(() => {
        cmdPalOpen.subscribe((val) => {
            if (val) {
                open();
            } else {
                close();
            }
        });

        cmdPalCommands.subscribe((cmds) => {
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
                }
                break;

            case "ArrowUp":
                {
                    e.preventDefault();

                    const index = filteredCommands.findIndex((cmd) => cmd === selectedCommand);
                    let newIndex = index - 1;
                    if (newIndex < 0) newIndex = filteredCommands.length - 1;

                    selectedCommand = filteredCommands[newIndex];
                }
                break;

            case "ArrowDown":
                {
                    e.preventDefault();

                    const index = filteredCommands.findIndex((cmd) => cmd === selectedCommand);
                    const newIndex = (index + 1) % filteredCommands.length;

                    selectedCommand = filteredCommands[newIndex];
                }
                break;
        }
    };

    const filter = () => {
        filteredCommands = commands.filter((cmd) => {
            if (cmd.name.toLowerCase().indexOf(search.toLowerCase()) > -1) {
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

        searchElement?.focus();

        if (!selectedCommand) {
            selectedCommand = commands[0];
        }

        cmdPalOpen.set(true);
    };

    const close = () => {
        isOpen = false;

        search = "";

        cmdPalOpen.set(false);
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

            {#each filteredCommands as cmd}
                <button
                    class="command"
                    on:click={() => onCmd(cmd)}
                    on:mousedown={() => onCmdMouseDown(cmd)}
                    on:mouseup={onCmdMouseUp}
                    class:selected={selectedCommand === cmd}
                >
                    <p>{cmd.name}</p>
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

            &:hover {
                background-color: $color-bg-1;
            }

            &.selected {
                background-color: $color-bg-2;
            }

            p {
                margin: 0;
            }
        }
    }
</style>
