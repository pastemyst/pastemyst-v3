<script lang="ts">
    import { onMount } from "svelte";
    import { basicSetup } from "codemirror";
    import { EditorView, keymap } from "@codemirror/view";
    import { indentWithTab } from "@codemirror/commands";
    import { mystCMTheme } from "./codemirror-myst-theme";
    import { languages as cmLangs } from "@codemirror/language-data";
    import { indentUnit, type LanguageDescription } from "@codemirror/language";
    import { indentSelect, langSelect, SelectCommand } from "./cmdOptions";
    import { isCommandPaletteOpen } from "./stores";
    import { Compartment, EditorState } from "@codemirror/state";
    import { getLangs, type Language } from "./api/lang";
    import { tooltip } from "$lib/tooltips";

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
    let selectedIndentWidth: number = 4;

    let previewEnabled = false;
    let currentPreviewContent: string;
    let langSupported = false;

    onMount(async () => {
        selectedLanguage = (await getLangs())[0];

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

        // if the current editor is focused and the command palette is closed, set the selected language
        // also focus the editor back (by default it will focus the lang button)
        // also set the indentation settings
        isCommandPaletteOpen.subscribe(async (open) => {
            if (hidden) return;

            // if opening, set all selected items and return
            if (open) {
                langSelect.setSelected(selectedLanguage.name);

                const selectedIndent = indentSelect.subCommands.find(
                    (s) => s.name === selectedIndentUnit
                ) as SelectCommand;
                const otherIndent = indentSelect.subCommands.find(
                    (s) => s.name !== selectedIndentUnit
                ) as SelectCommand;

                selectedIndent.setSelected(selectedIndentWidth.toString());

                const otherIndentSelected = otherIndent.getSelected();
                if (otherIndentSelected) {
                    otherIndentSelected.selected = false;
                }

                return;
            }

            selectedLanguage = (await getLangs()).find(
                (l) => l.name.toLowerCase() === langSelect.getSelected()?.name.toLowerCase()
            )!;

            let langDescription = cmLangs.find(
                (l) => selectedLanguage.name.toLowerCase() === l.name.toLowerCase()
            );

            if (langDescription) {
                langSupported = true;

                let langSupport = await langDescription.load();

                editorView.dispatch({
                    effects: langCompartment.reconfigure(langSupport)
                });
            } else {
                langSupported = false;
                if (selectedLanguage.name === "Text") langSupported = true;

                editorView.dispatch({
                    effects: langCompartment.reconfigure([])
                });
            }

            focus();

            const tabsOpt = indentSelect.subCommands.find(
                (s) => s.name === "tabs"
            ) as SelectCommand;
            const spacesOpt = indentSelect.subCommands.find(
                (s) => s.name === "spaces"
            ) as SelectCommand;

            const tabsWidth = tabsOpt.getSelected();
            const spacesWidth = spacesOpt.getSelected();

            // if both are selected, that means the unit was changed
            // otherwise only the width was changed
            if (tabsWidth && spacesWidth) {
                selectedIndentUnit = selectedIndentUnit === "spaces" ? "tabs" : "spaces";
                selectedIndentWidth =
                    selectedIndentUnit === "spaces"
                        ? Number(spacesWidth.name)
                        : Number(tabsWidth.name);
            }

            selectedIndentWidth =
                selectedIndentUnit === "spaces"
                    ? Number(spacesWidth?.name)
                    : Number(tabsWidth?.name);

            setEditorIndentation();
        });
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

    const onLanguageClick = () => {
        const evt = new CustomEvent("cmdShowOptions", { detail: langSelect });
        window.dispatchEvent(evt);
    };

    const onIndentClick = () => {
        const evt = new CustomEvent("cmdShowOptions", { detail: indentSelect });
        window.dispatchEvent(evt);
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
        editorView.focus();
    };

    export const getContent = (): string => {
        return editorView.state.doc.toString();
    };

    export const getSelectedLang = (): Language => {
        return selectedLanguage;
    };

    export const setSelectedLang = (lang: Language) => {
        selectedLanguage = lang;
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
                <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                    <title>Information Circle</title>
                    <path
                        fill="currentColor"
                        d="M256 56C145.72 56 56 145.72 56 256s89.72 200 200 200 200-89.72 200-200S366.28 56 256 56zm0 82a26 26 0 11-26 26 26 26 0 0126-26zm48 226h-88a16 16 0 010-32h28v-88h-16a16 16 0 010-32h32a16 16 0 0116 16v104h28a16 16 0 010 32z"
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
            border: 1px solid $color-bg-2;
            @include transition();

            &:hover {
                background-color: $color-bg-2;
                border-color: $color-bg-3;

                :global(.cm-gutter),
                :global(.cm-activeLineGutter) {
                    background-color: $color-bg-2;
                }
            }
        }

        :global(.cm-focused) {
            border-color: $color-prim !important;
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
        background-color: $color-sec;
        margin: 0.5rem;
        padding: 0.25rem 0.5rem;
        border-radius: $border-radius;
        color: $color-bg-2;

        .icon {
            color: $color-bg-2;
            margin-right: 0.25rem;
        }

        p {
            margin: 0;
        }
    }

    .toolbar {
        font-size: $fs-small;
        background-color: $color-bg-2;
        padding: 0.25rem 0.5rem;
        border-radius: 0 0 $border-radius $border-radius;

        button {
            margin-right: 0.5rem;

            &.enabled {
                color: $color-sec;
                border-color: $color-sec;
            }
        }
    }

    .preview {
        height: 50vh;
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
        border: 1px solid $color-bg-2;
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

    // TODO: temporary line numbers, they should be interactive and not style only
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
        color: $color-bg-3;
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
