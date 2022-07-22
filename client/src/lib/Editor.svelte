<script lang="ts">
    import { onMount } from "svelte";
    import { basicSetup } from "codemirror";
    import { EditorView, keymap } from "@codemirror/view";
    import { indentWithTab } from "@codemirror/commands";
    import { mystTheme, mystHighlightStyle } from "./codemirror-myst-theme";
    import { languages } from "@codemirror/language-data";
    import { indentUnit, type LanguageDescription } from "@codemirror/language";
    import { indentSelect, langSelect, SelectCommand } from "./cmdOptions";
    import { isCommandPaletteOpen } from "./stores";
    import { Compartment, EditorState } from "@codemirror/state";

    type IndentUnit = "tabs" | "spaces";

    export let hidden = false;

    let editorElement: HTMLElement;

    let editorView: EditorView;

    let cursorLine = 0;
    let cursorCol = 0;

    let langCompartment = new Compartment();
    let selectedLanguage: LanguageDescription = languages.sort((a, b) =>
        a.name.localeCompare(b.name)
    )[0];

    let indentUnitCompartment = new Compartment();
    let indentWidthCompartment = new Compartment();
    let selectedIndentUnit: IndentUnit = "spaces";
    let selectedIndentWidth: number = 4;

    onMount(async () => {
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
                    mystTheme,
                    mystHighlightStyle,
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
                const otherIndent = indentSelect.subCommands.find((s) => s.name !== selectedIndentUnit) as SelectCommand;

                selectedIndent.setSelected(selectedIndentWidth.toString());

                const otherIndentSelected = otherIndent.getSelected();
                if (otherIndentSelected) {
                    otherIndentSelected.selected = false;
                }

                return;
            }

            selectedLanguage = languages.find(
                (l) => l.name.toLowerCase() === langSelect.getSelected()?.name.toLowerCase()
            )!;

            let langSupport = await selectedLanguage.load();

            editorView.dispatch({
                effects: langCompartment.reconfigure(langSupport)
            });

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

    export const focus = (): void => {
        editorView.focus();
    };

    export const getContent = (): string => {
        return editorView.state.doc.toString();
    };

    export const getSelectedLang = (): LanguageDescription => {
        return selectedLanguage;
    };

    export const setSelectedLang = (lang: LanguageDescription) => {
        selectedLanguage = lang;
    };
</script>

<div class:hidden>
    <div class="editor" bind:this={editorElement} />

    <div class="toolbar flex sm-row center space-between">
        <div class="flex row center">
            <button on:click={onLanguageClick}>language: {selectedLanguage.name}</button>

            <button on:click={onIndentClick}>{selectedIndentUnit}: {selectedIndentWidth}</button>
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

    .toolbar {
        font-size: $fs-small;
        background-color: $color-bg-2;
        padding: 0.25rem 0.5rem;
        border-radius: 0 0 $border-radius $border-radius;

        button {
            margin-right: 0.5rem;
        }
    }

    @media screen and (max-width: 620px) {
        .toolbar .line {
            margin-top: 0.5rem;
            padding-left: 0.25rem;
        }
    }
</style>
