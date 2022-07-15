<script lang="ts">
    import highlightWords, { HighlightWords } from "highlight-words";
    import { onMount } from "svelte";
    import { isCommandPaletteOpen } from "./stores";

    class Command {
        name: string;
        icon?: string = null;
        description?: string = null;
        shortcuts?: string[][] = [];

        constructor(name: string) {
            this.name = name;
        }

        withName(name: string): Command {
            this.name = name;
            return this;
        }

        withIcon(icon: string): Command {
            this.icon = icon;
            return this;
        }

        withDescription(description: string): Command {
            this.description = description;
            return this;
        }

        addShortcut(shortcut: string[]): Command {
            this.shortcuts.push(shortcut);
            return this;
        }
    }

    class LinkCommand extends Command {
        url: string;

        constructor(name: string) {
            super(name);
        }

        withUrl(url: string): LinkCommand {
            this.url = url;
            return this;
        }
    }

    class DirCommand extends Command {
        subCommands: Command[];

        constructor(name: string) {
            super(name);
        }

        withCommands(commands: Command[]) {
            this.subCommands = commands;
            return this;
        }
    }

    export let isOpen: boolean = false;

    let searchElement: HTMLInputElement;
    let search: string;
    let searchFound: boolean = true;

    let isCommandSelected: boolean = false;

    const gitCommands: Command[] = [];

    const githubCmd = new LinkCommand("github")
        .withUrl("https://github.com/")
        .withDescription("this one has a description")
        .addShortcut(["ctrl", "g"])
        .addShortcut(["ctrl", "p"]);

    const gitlabCmd = new LinkCommand("gitlab")
        .withUrl("https://gitlab.com/")
        .addShortcut(["ctrl", "l"]);

    const bitbucketCmd = new LinkCommand("bitbucket")
        .withUrl("https://bitbucket.com/")
        .addShortcut(["ctrl", "b"]);

    const gitmystCmd = new LinkCommand("gitmyst").withUrl("https://git.myst.rs/");

    const sourcehutCmd = new LinkCommand("sourcehut").withUrl("https://sr.ht/");

    gitCommands.push(githubCmd);
    gitCommands.push(gitlabCmd);
    gitCommands.push(bitbucketCmd);
    gitCommands.push(gitmystCmd);
    gitCommands.push(sourcehutCmd);

    const editorCommands: Command[] = [];

    const vscodeCmd = new LinkCommand("vscode").withUrl("/");

    const vimCmd = new LinkCommand("vim").withUrl("/");

    const sublimeCmd = new LinkCommand("sublime").withUrl("/");

    editorCommands.push(vscodeCmd);
    editorCommands.push(vimCmd);
    editorCommands.push(sublimeCmd);

    const editorCommand = new DirCommand("editors").withCommands(editorCommands);
    const gitCommand = new DirCommand("git services").withCommands(gitCommands);

    const rootCommands: Command[] = [editorCommand, gitCommand];

    let commands: Command[] = rootCommands;
    let filteredCommands: Command[] = commands;
    let highlightedChunks: HighlightWords.Chunk[][][];

    let elements: HTMLElement[] = [];
    let selectedCommand: Command;

    onMount(() => {
        isCommandPaletteOpen.subscribe(async (open) => await setOpen(open, false));
    });

    const setOpen = async (v: boolean, updateStore = true) => {
        isOpen = v;

        if (isOpen) {
            selectedCommand = filteredCommands[0];

            searchElement?.focus();
            search = "";
        } else {
            commands = rootCommands;
            filteredCommands = commands;
        }

        if (updateStore) {
            isCommandPaletteOpen.set(v);
        }
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
                }
                break;

            case "ArrowDown":
                {
                    e.preventDefault();
                    const index = filteredCommands.findIndex((e) => e === selectedCommand);
                    const newIndex = (index + 1) % filteredCommands.length;
                    selectedCommand = filteredCommands[newIndex];
                }
                break;
        }
    };

    const onDirClick = (e: MouseEvent, dir: DirCommand) => {
        e.preventDefault();
        changeDir(dir);
    };

    const changeDir = (dir: DirCommand) => {
        commands = dir.subCommands;
        filteredCommands = commands;
        selectedCommand = commands[0];
        search = "";
        searchElement.focus();
    };

    const filter = () => {
        filteredCommands = commands.filter(
            (e) =>
                e.name.toLowerCase().indexOf(search.toLowerCase()) > -1 ||
                e.description?.toLowerCase().indexOf(search.toLowerCase()) > -1
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
        <div class="commands">
            {#each filteredCommands as cmd, cmdIndex}
                <a
                    href={cmd instanceof LinkCommand ? cmd.url : null}
                    class="command flex col no-dec"
                    target="_blank"
                    on:click={cmd instanceof DirCommand ? (e) => onDirClick(e, cmd) : null}
                    on:mousedown={() => onCommandMouseDown(cmd)}
                    on:mouseup={onCommandMouseUp}
                    class:selected={selectedCommand === cmd}
                    bind:this={elements[cmdIndex]}
                >
                    <div class="name flex row center">
                        <!-- icon -->
                        {#if cmd.icon}
                            <div class="icon flex row center">
                                {@html cmd.icon}
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
        overflow: hidden;

        .command {
            color: $color-fg;
            padding: 0.5rem;
            cursor: pointer;

            &.selected,
            &:hover {
                background-color: $color-bg-1;
                border-left: 2px solid $color-sec;
                padding-left: 0.75rem;
            }

            .highlight {
                color: $color-sec;
            }

            .name {
                .icon {
                    margin-right: 1rem;
                    color: $color-fg;
                    max-width: 25px;
                }
            }

            .description {
                color: $color-bg-3;
                font-size: $fs-small;

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
