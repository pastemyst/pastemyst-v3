<script lang="ts">
    import { onMount } from "svelte";
    import type { Command } from "./command";
    import { cmdPalCommands, cmdPalOpen } from "./stores";

    let isOpen = false;

    let commands: Command[] = [];

    let searchElement: HTMLInputElement | undefined;

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
        });
    })

    const handleKeys = (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key === "k") {
            e.preventDefault();
            toggle();
        }
    };

    const onSearchBlur = () => {
        if (selectedCommand) return;

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

        cmdPalOpen.set(true);
    };

    const close = () => {
        isOpen = false;

        if (searchElement) {
            searchElement.value = "";
        }

        cmdPalOpen.set(false);
    };

    const onCmd = (cmd: Command) => {
        cmd.action();

        close();
    };

    const onCmdMouseDown = (cmd: Command) => {
        selectedCommand = cmd;
    };

    const onCmdMouseUp = () => {
        selectedCommand = undefined;
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
                on:blur={onSearchBlur}
                on:keydown={onSearchKeyDown}
            />
        </div>

        <div class="commands">
            {#if commands.length === 0}
                <p class="no-commands">there aren't any commands defined.</p>
            {/if}

            {#each commands as cmd}
                <button class="command" on:click={() => onCmd(cmd)} on:mousedown={() => onCmdMouseDown(cmd)} on:mouseup={onCmdMouseUp}>
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
                background-color: $color-bg-2;
            }

            p {
                margin: 0;
            }
        }
    }
</style>
