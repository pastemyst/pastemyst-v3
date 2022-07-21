<script lang="ts">
    import { onMount } from "svelte";
    import { EditorState, basicSetup } from "@codemirror/basic-setup";
    import { EditorView, keymap } from "@codemirror/view";
    import { indentWithTab } from "@codemirror/commands";
    import { myst } from "./codemirror-myst-theme";

    export let hidden = false;

    let editorElement: HTMLElement;

    let editorView: EditorView;

    let cursorLine = 0;
    let cursorCol = 0;

    onMount(async () => {
        const editorUpdateListener = EditorView.updateListener.of((update) => {
            // get the current line
            const line = update.state.doc.lineAt(update.state.selection.main.head);

            cursorLine = line.number;
            cursorCol = update.state.selection.main.head - line.from;
        });

        editorView = new EditorView({
            state: EditorState.create({
                extensions: [basicSetup, keymap.of([indentWithTab]), myst, editorUpdateListener]
            }),
            parent: editorElement
        });
    });

    export const focus = (): void => {
        editorView.focus();
    };

    export const getContent = (): string => {
        return editorView.state.doc.toString();
    };
</script>

<div class:hidden>
    <div class="editor" bind:this={editorElement} />

    <div class="toolbar flex row center space-between">
        <div />

        <div class="flex row center">
            <div class="line element">
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
    }
</style>
