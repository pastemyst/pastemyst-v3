<script lang="ts">
    import { isCommandPaletteOpen } from "../stores";
    import { onMount, tick } from "svelte";
    import highlightWords, { HighlightWords } from "highlight-words";

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
        .withIcon("logo-github")
        .withDescription("this one has a description")
        .addShortcut(["ctrl", "g"])
        .addShortcut(["ctrl", "p"]);

    const gitlabCmd = new LinkCommand("gitlab")
        .withUrl("https://gitlab.com/")
        .withIcon("logo-gitlab")
        .addShortcut(["ctrl", "l"]);

    const bitbucketCmd = new LinkCommand("bitbucket")
        .withUrl("https://bitbucket.com/")
        .withIcon("logo-bitbucket")
        .addShortcut(["ctrl", "b"]);

    const gitmystCmd = new LinkCommand("gitmyst").withUrl("https://git.myst.rs/").withIcon("flask");

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

            await tick();

            searchElement.focus();
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
            case "Enter": {
                const index = filteredCommands.findIndex((e) => e === selectedCommand);
                elements[index].click();
            } break;

            case "ArrowUp": {
                e.preventDefault();
                const index = filteredCommands.findIndex((e) => e === selectedCommand);
                selectedCommand = index > 0 ? filteredCommands[index - 1] : filteredCommands[0];
            } break;

            case "ArrowDown": {
                e.preventDefault();
                const index = filteredCommands.findIndex((e) => e === selectedCommand);
                selectedCommand = index < filteredCommands.length - 1 ? filteredCommands[index + 1] : filteredCommands[filteredCommands.length - 1];
            } break;
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
        filteredCommands = commands.filter(e => e.name.toLowerCase().indexOf(search.toLowerCase()) > -1 ||
                                                e.description?.toLowerCase().indexOf(search.toLowerCase()) > -1);
        searchFound = filteredCommands.length > 0;

        if (searchFound) {
            selectedCommand = filteredCommands[0];
        }

        highlightedChunks = [];

        for (const cmd of filteredCommands) {
            const chunks = [];

            chunks.push(highlightWords({
                text: cmd.name,
                query: search,
                matchExactly: true
            }));

            if (cmd.description) {
                chunks.push(highlightWords({
                    text: cmd.description,
                    query: search,
                    matchExactly: true
                }));
            }

            highlightedChunks.push(chunks);
        }
    };
</script>

<div role="dialog" aria-modal="true" class="palette" class:isOpen>
    <div class="wrapper">
        <div class="search flex row center">
            <ion-icon name="search" />

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
                            <ion-icon name={cmd.icon} />
                        {/if}

                        <!-- name -->
                        {#if search !== undefined && search !== ""}
                            {#each highlightedChunks[cmdIndex][0] as chunk (chunk.key)}
                                <span aria-hidden="true" class:highlight={chunk.match}>{chunk.text}</span>
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
                                <span aria-hidden="true" class:highlight={chunk.match}>{chunk.text}</span>
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
        background-color: rgba(0, 0, 0, 0.5);
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
        margin-top: 1.5rem;
        border-radius: $border-radius;
    }

    .search {
        padding: 0.5rem;

        ion-icon {
            margin-right: 1rem;
        }

        input {
            background: transparent;
            border: none;
            outline: none;
            color: $color-fg;
            width: 100%;
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
            }

            span {
                white-space: pre;
            }

            .highlight {
                color: $color-sec;
            }

            .name {
                margin-bottom: 0.25rem;

                ion-icon {
                    margin-right: 1rem;
                }
            }

            .description {
                color: $color-bg-3;
                font-size: $fs-small;
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
