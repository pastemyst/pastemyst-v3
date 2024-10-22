<script lang="ts">
    import { onMount } from "svelte";
    import type { Pasty } from "./api/paste";
    import { MergeView } from "@codemirror/merge";
    import { EditorView } from "codemirror";
    import { Compartment, EditorState, type Extension } from "@codemirror/state";
    import { themes } from "./themes";
    import { languages as cmLangs } from "@codemirror/language-data";
    import type { Settings } from "./api/settings";
    import { drawSelection, highlightSpecialChars, lineNumbers } from "@codemirror/view";

    export let settings: Settings;
    export let oldPasty: Pasty | undefined = undefined;
    export let newPasty: Pasty | undefined = undefined;

    let editor: MergeView;

    let oldPastyLanguageCompartment = new Compartment();
    let newPastyLanguageCompartment = new Compartment();

    let editorElement: HTMLElement;

    onMount(async () => {
        const baseTheme = EditorView.theme({
            "*": {
                fontFamily: '"Ubuntu Mono", monospace'
            }
        });

        const codemirrorTheme = (themes.find((t) => t.name === settings.theme) || themes[0])
            .codemirrorTheme;

        const extensions: Extension = [
            baseTheme,
            codemirrorTheme,
            lineNumbers(),
            highlightSpecialChars(),
            drawSelection(),
            EditorView.editable.of(false),
            EditorState.readOnly.of(true)
        ];

        editor = new MergeView({
            a: {
                doc: oldPasty?.content ?? "",
                extensions: [...extensions, oldPastyLanguageCompartment.of([])]
            },
            b: {
                doc: newPasty?.content ?? "",
                extensions: [...extensions, newPastyLanguageCompartment.of([])]
            },
            parent: editorElement
        });

        const oldPastyLanguageDescription = cmLangs.find(
            (l) => oldPasty?.language.toLowerCase() === l.name.toLowerCase()
        );

        if (oldPastyLanguageDescription) {
            const languageSupport = await oldPastyLanguageDescription.load();

            editor.a.dispatch({
                effects: oldPastyLanguageCompartment.reconfigure(languageSupport.language)
            });
        }

        const newPastyLanguageDescription = cmLangs.find(
            (l) => newPasty?.language.toLowerCase() === l.name.toLowerCase()
        );

        if (newPastyLanguageDescription) {
            const languageSupport = await newPastyLanguageDescription.load();

            editor.b.dispatch({
                effects: newPastyLanguageCompartment.reconfigure(languageSupport.language)
            });
        }
    });
</script>

<div class="title flex row space-between">
    <div class="old">
        {#if oldPasty}
            <span class:deleted={!newPasty || oldPasty.title !== newPasty.title}
                >{oldPasty.title}</span
            >
        {/if}
    </div>

    <div class="new">
        {#if newPasty}
            <span class:added={!oldPasty || newPasty.title !== oldPasty.title}
                >{newPasty.title}</span
            >
        {/if}
    </div>
</div>

<div class="editor" bind:this={editorElement}></div>

<style lang="scss">
    .title {
        width: 100%;
        background-color: var(--color-bg1);
        border-top-left-radius: $border-radius;
        border-top-right-radius: $border-radius;
        border: 1px solid var(--color-bg2);
        margin-top: 1rem;
        font-size: $fs-normal;

        .old {
            width: 50%;
            border-right: 1px solid var(--color-bg2);
            padding: 0.25rem 0.5rem;
        }

        .new {
            width: 50%;
            padding: 0.25rem 0.5rem;
        }

        span.deleted {
            color: var(--color-danger);
        }

        span.added {
            color: var(--color-success);
        }
    }

    .editor {
        border-left: 1px solid var(--color-bg2);
        border-right: 1px solid var(--color-bg2);
        border-bottom: 1px solid var(--color-bg2);
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
    }
</style>
