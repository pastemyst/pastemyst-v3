<script lang="ts">
    import { onMount } from "svelte";
    import { basicSetup } from "codemirror";
    import { EditorView, keymap } from "@codemirror/view";
    import { indentWithTab } from "@codemirror/commands";
    import { mystCMTheme } from "./codemirror-myst-theme";
    import { indentUnit } from "@codemirror/language";
    import { Compartment, EditorState } from "@codemirror/state";
    import { getLangs, type Language } from "./api/lang";
    import { tooltip } from "$lib/tooltips";
    import { Close, setTempCommands, type Command } from "./command";
    import { cmdPalOpen } from "./stores";
    import { languages as cmLangs } from "@codemirror/language-data";

    type IndentUnit = "tabs" | "spaces";

    export let hidden = false;

    let editorElement: HTMLElement;

    let editorView: EditorView;

    let cursorLine = 0;
    let cursorCol = 0;

    let langCompartment = new Compartment();
    let selectedLanguage: Language;

    let indentUnitCompartment = new Compartment();
    let indentWidthCompartment = new Compartment();
    let selectedIndentUnit: IndentUnit = "spaces";
    let selectedIndentWidth = 4;

    let previewEnabled = false;
    let currentPreviewContent: string;
    let langSupported = false;

    let langs: Language[];

    onMount(() => {
        (async () => {
            langs = await getLangs();
            selectedLanguage = langs[0];
        })();

        const editorUpdateListener = EditorView.updateListener.of((update) => {
            // get the current line
            const line = update.state.doc.lineAt(update.state.selection.main.head);

            cursorLine = line.number;
            cursorCol = update.state.selection.main.head - line.from;
        });

        editorView = new EditorView({
            state: EditorState.create({
                extensions: [
                    basicSetup,
                    keymap.of([indentWithTab]),
                    mystCMTheme,
                    editorUpdateListener,
                    langCompartment.of([]),
                    indentUnitCompartment.of(
                        indentUnit.of(selectedIndentUnit === "spaces" ? " " : "\t")
                    ),
                    indentWidthCompartment.of(EditorState.tabSize.of(selectedIndentWidth))
                ]
            }),
            parent: editorElement
        });

        setEditorIndentation();

        focus();
    });

    const setEditorIndentation = () => {
        if (selectedIndentUnit === "spaces") {
            editorView.dispatch({
                effects: [
                    indentUnitCompartment.reconfigure(
                        indentUnit.of(" ".repeat(selectedIndentWidth))
                    )
                ]
            });
        } else {
            editorView.dispatch({
                effects: [
                    indentUnitCompartment.reconfigure(indentUnit.of("\t")),
                    indentWidthCompartment.reconfigure(EditorState.tabSize.of(selectedIndentWidth))
                ]
            });
        }
    };

    export const replaceIndentation = (previousUnit: IndentUnit, previousWidth: number) => {
        const { lines } = editorView.state.doc;

        for (let i = 1; i <= lines; i++) {
            const { from, to, text } = editorView.state.doc.line(i);

            const currentIndentLength = text.slice(0, text.indexOf(text.trimStart()[0])).length;

            const previousSpaces = Math.floor(currentIndentLength / previousWidth);

            const transaction = editorView.state.update({
                changes: {
                    from,
                    to,
                    insert:
                        selectedIndentUnit === "spaces"
                            ? " ".repeat(
                                  previousUnit === "spaces"
                                      ? selectedIndentWidth * previousSpaces
                                      : currentIndentLength * selectedIndentWidth
                              ) + text.trimStart()
                            : "\t".repeat(
                                  previousUnit === "spaces" ? previousSpaces : currentIndentLength
                              ) + text.trimStart()
                }
            });

            editorView.update([transaction]);
        }
    };

    export const getLanguageCommands = async (): Promise<Command[]> => {
        const commands: Command[] = [];

        langs = await getLangs();

        for (const lang of langs) {
            commands.push({
                name: lang.name,
                description: lang.aliases?.join(", "),
                action: () => {
                    setSelectedLang(lang);

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    export const getIndentUnitCommands = (convertIndent = false): Command[] => {
        return [
            {
                name: "spaces",
                action: () => {
                    let prevUnit = selectedIndentUnit;
                    selectedIndentUnit = "spaces";
                    setEditorIndentation();
                    setTempCommands(getIndentWidthCommands(prevUnit, convertIndent));

                    return Close.no;
                }
            },
            {
                name: "tabs",
                action: () => {
                    let prevUnit = selectedIndentUnit;
                    selectedIndentUnit = "tabs";
                    setEditorIndentation();
                    setTempCommands(getIndentWidthCommands(prevUnit, convertIndent));

                    return Close.no;
                }
            }
        ];
    };

    export const getIndentWidthCommands = (
        prevUnit: IndentUnit,
        convertIndent = false
    ): Command[] => {
        const commands: Command[] = [];

        for (let i = 1; i <= 8; i++) {
            commands.push({
                name: i.toString(),
                action: () => {
                    let prevWidth = selectedIndentWidth;
                    selectedIndentWidth = i;
                    setEditorIndentation();

                    if (convertIndent) {
                        replaceIndentation(prevUnit, prevWidth);
                    }

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const onLanguageClick = async () => {
        setTempCommands(await getLanguageCommands());

        cmdPalOpen.set(true);
    };

    const onIndentClick = () => {
        setTempCommands(getIndentUnitCommands());

        cmdPalOpen.set(true);
    };

    const onPreviewClick = async () => {
        const res = await fetch("/internal/highlight", {
            method: "post",
            body: JSON.stringify({
                content: getContent(),
                language: getSelectedLang().name
            })
        });

        currentPreviewContent = await res.text();

        previewEnabled = !previewEnabled;
    };

    export const focus = (): void => {
        editorView?.focus();
    };

    export const getContent = (): string => {
        return editorView.state.doc.toString();
    };

    export const setContent = (content: string) => {
        const transaction = editorView.state.update({
            changes: { from: 0, to: editorView.state.doc.length, insert: content }
        });

        editorView.update([transaction]);
    };

    export const getSelectedLang = (): Language => {
        return selectedLanguage;
    };

    export const setSelectedLang = (lang: Language) => {
        selectedLanguage = lang;

        const langDescription = cmLangs.find(
            (l) => selectedLanguage.name.toLowerCase() === l.name.toLowerCase()
        );

        if (langDescription) {
            langSupported = true;

            langDescription.load().then((langSupport) => {
                editorView.dispatch({
                    effects: langCompartment.reconfigure(langSupport)
                });
            });
        } else {
            langSupported = false;
            if (selectedLanguage.name === "Text") langSupported = true;

            editorView.dispatch({
                effects: langCompartment.reconfigure([])
            });
        }
    };
</script>

<div class:hidden>
    {#if previewEnabled}
        <div class="preview">
            {@html currentPreviewContent}
        </div>
    {/if}

    <div class="editor" bind:this={editorElement} class:hidden={previewEnabled}>
        {#if !langSupported}
            <div
                class="lang-not-supported flex row center"
                use:tooltip
                aria-label="the language doesn't have highlighting support in the editor, but will have it in
                the actual paste view when the paste is created. use the preview button to see the
                final result."
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Info Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M8 1.5a6.5 6.5 0 100 13 6.5 6.5 0 000-13zM0 8a8 8 0 1116 0A8 8 0 010 8zm6.5-.25A.75.75 0 017.25 7h1a.75.75 0 01.75.75v2.75h.25a.75.75 0 010 1.5h-2a.75.75 0 010-1.5h.25v-2h-.25a.75.75 0 01-.75-.75zM8 6a1 1 0 100-2 1 1 0 000 2z"
                    />
                </svg>
                <p>limited language support</p>
            </div>
        {/if}
    </div>

    <div class="toolbar flex sm-row center space-between">
        <div class="flex sm-row center">
            <button on:click={onLanguageClick}>language: {selectedLanguage?.name}</button>

            <button on:click={onIndentClick}>{selectedIndentUnit}: {selectedIndentWidth}</button>

            <button on:click={onPreviewClick} class:enabled={previewEnabled}>preview</button>
        </div>

        <div class="flex row center">
            <div class="line">
                ln {cursorLine} col {cursorCol}
            </div>
        </div>
    </div>
</div>

<style lang="scss">
    .hidden {
        display: none;
    }

    .editor {
        height: 50vh;
        position: relative;
        font-size: $fs-normal;

        :global(.cm-editor) {
            border: 1px solid var(--color-bg2);
            @include transition();

            &:hover {
                background-color: var(--color-bg2);
                border-color: var(--color-bg3);

                :global(.cm-gutter),
                :global(.cm-activeLineGutter) {
                    background-color: var(--color-bg2);
                }
            }
        }

        :global(.cm-focused) {
            border-color: var(--color-primary) !important;
        }

        :global(.cm-scroller) {
            border-radius: $border-radius;
        }

        :global(.cm-gutter),
        :global(.cm-activeLineGutter) {
            @include transition();
        }

        :global(.cm-editor:focus),
        :global(.cm-focused) {
            outline: none !important;
        }

        :global(.cm-editor),
        :global(.cm-wrap) {
            height: 100%;
        }

        :global(.cm-scroller) {
            overflow: auto;
        }
    }

    .lang-not-supported {
        position: absolute;
        z-index: 100;
        user-select: none;
        font-size: $fs-small;
        box-sizing: border-box;
        bottom: 0;
        right: 0;
        background-color: var(--color-secondary);
        margin: 0.5rem;
        padding: 0.25rem 0.5rem;
        border-radius: $border-radius;
        color: var(--color-bg2);

        .icon {
            color: var(--color-bg2);
            margin-right: 0.5rem;
        }

        p {
            margin: 0;
        }
    }

    .toolbar {
        font-size: $fs-small;
        background-color: var(--color-bg2);
        padding: 0.25rem 0.5rem;
        border-radius: 0 0 $border-radius $border-radius;

        button {
            margin-right: 0.5rem;

            &.enabled {
                color: var(--color-secondary);
                border-color: var(--color-secondary);
            }
        }
    }

    .preview {
        height: 50vh;
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
        border: 1px solid var(--color-bg2);
        border-top: none;
        margin: 0;
        overflow-x: auto;
        padding: 0;
        padding-top: 0.2rem;
        padding-left: 0.05rem;
    }

    :global(.shiki) {
        margin: 0;
    }

    :global(.shiki code) {
        border: none;
        font-size: $fs-normal;
        padding: 0;
        border-radius: 0;
        background-color: transparent;
    }

    :global(.shiki code) {
        counter-reset: step;
        counter-increment: step 0;
    }

    :global(.shiki code .line::before) {
        content: counter(step);
        counter-increment: step;
        width: 1rem;
        margin-right: 1.1rem;
        display: inline-block;
        text-align: right;
        color: var(--color-bg3);
        font-size: $fs-normal;
        padding-left: 0.75rem;
    }

    @media screen and (max-width: 620px) {
        .toolbar .line {
            margin-top: 0.5rem;
            padding-left: 0.25rem;
        }

        .toolbar button {
            margin-top: 0.5rem;
        }
    }
</style>
