<script lang="ts">
    import { onMount } from "svelte";
    import { basicSetup } from "codemirror";
    import { EditorView, keymap } from "@codemirror/view";
    import { indentWithTab } from "@codemirror/commands";
    import { indentUnit } from "@codemirror/language";
    import { Compartment, EditorState } from "@codemirror/state";
    import { getLangs, getPopularLangNames, autodetectLanguage, type Language } from "./api/lang";
    import { tooltip } from "$lib/tooltips";
    import { Close, setTempCommands, type Command } from "./command";
    import { cmdPalOpen, cmdPalTitle, themeStore } from "./stores.svelte";
    import { languages as cmLangs } from "@codemirror/language-data";
    import { isLanguageMarkdown } from "./utils/markdown";
    import type { IndentUnit } from "./indentation";
    import type { Settings } from "./api/settings";
    import { themes } from "./themes";
    import { marked } from "marked";

    interface Props {
        hidden?: boolean;
        settings: Settings;
        onMounted?: () => void;
    }

    let { hidden = false, settings, onMounted = undefined }: Props = $props();

    let editorElement: HTMLElement;

    let editorView: EditorView;

    let cursorLine = $state(0);
    let cursorCol = $state(0);

    let langCompartment = new Compartment();
    let selectedLanguage: Language | undefined = $state();

    let indentUnitCompartment = new Compartment();
    let indentWidthCompartment = new Compartment();
    let selectedIndentUnit: IndentUnit = $state("spaces");
    let selectedIndentWidth = $state(4);

    let themeCompartment = new Compartment();

    let previewEnabled = $state(false);
    let currentPreviewContent: string = $state("");
    let langSupported = $state(true);

    let langs: Language[];

    themeStore.subscribe((theme) => {
        if (!theme || !editorView) return;

        const codemirrorTheme = (themes.find((t) => t.name === theme.name) || themes[0])
            .codemirrorTheme;

        editorView.dispatch({
            effects: themeCompartment.reconfigure(codemirrorTheme)
        });
    });

    onMount(async () => {
        langs = await getLangs(fetch);

        const popularLangs = await getPopularLangNames(fetch);

        // make sure popular languages are at the top
        langs.sort((a, b) => {
            const aPopular = popularLangs.includes(a.name) ? 1 : 0;
            const bPopular = popularLangs.includes(b.name) ? 1 : 0;

            return bPopular - aPopular;
        });

        const textLangIndex = langs.findIndex((l) => l.name === "Text");
        const textLang = langs[textLangIndex];

        // place text on the top of the lang list below autodetect
        langs.splice(textLangIndex, 1);
        langs.unshift(textLang);

        const autodetectLangIndex = langs.findIndex((l) => l.name === "Autodetect");
        const autodetectLang = langs[autodetectLangIndex];

        // place autodetect on the top of the lang list
        langs.splice(autodetectLangIndex, 1);
        langs.unshift(autodetectLang);

        if (!selectedLanguage) {
            selectedLanguage = autodetectLang;
        }

        selectedIndentUnit = settings.defaultIndentationUnit;
        selectedIndentWidth = settings.defaultIndentationWidth;

        const editorUpdateListener = EditorView.updateListener.of((update) => {
            // get the current line
            const line = update.state.doc.lineAt(update.state.selection.main.head);

            cursorLine = line.number;
            cursorCol = update.state.selection.main.head - line.from;
        });

        const autodetectLanguageOnPasteExtension = EditorView.domEventHandlers({
            paste(event) {
                if (selectedLanguage?.name !== "Autodetect") return;

                const content = event.clipboardData?.getData("text/plain");

                if (!content) return;

                autodetectLanguage(content).then((lang) => {
                    setSelectedLang(lang);
                });
            }
        });

        const codemirrorTheme = ($themeStore || themes[0]).codemirrorTheme;

        const baseTheme = EditorView.theme({
            "*": {
                fontFamily: '"Ubuntu Mono", monospace'
            }
        });

        // TODO: first create the view and then update it after fetching all the required things

        editorView = new EditorView({
            state: EditorState.create({
                extensions: [
                    baseTheme,
                    basicSetup,
                    keymap.of([indentWithTab]),
                    themeCompartment.of([codemirrorTheme]),
                    editorUpdateListener,
                    langCompartment.of([]),
                    indentUnitCompartment.of(
                        indentUnit.of(selectedIndentUnit === "spaces" ? " " : "\t")
                    ),
                    indentWidthCompartment.of(EditorState.tabSize.of(selectedIndentWidth)),
                    settings.textWrap ? [EditorView.lineWrapping] : [],
                    autodetectLanguageOnPasteExtension
                ]
            }),
            parent: editorElement
        });

        const settingsLang = langs.find((l) => l.name === settings.defaultLanguage) || textLang;
        setSelectedLang(settingsLang);

        setEditorIndentation();

        focus();

        if (onMounted) onMounted();
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

    export const setHidden = (h: boolean) => {
        hidden = h;
    };

    export const setIndentaion = (unit: IndentUnit, width: number) => {
        selectedIndentUnit = unit;
        selectedIndentWidth = width;
        setEditorIndentation();
    };

    export const getIndentation = (): [IndentUnit, number] => {
        return [selectedIndentUnit, selectedIndentWidth];
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

        langs = await getLangs(fetch);

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
                    cmdPalTitle.set("select indentation width (spaces)");
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
                    cmdPalTitle.set("select indentation width (tabs)");
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

        cmdPalTitle.set("select language");
        cmdPalOpen.set(true);
    };

    const onIndentClick = () => {
        setTempCommands(getIndentUnitCommands());

        cmdPalTitle.set("select indentation unit");
        cmdPalOpen.set(true);
    };

    const preview = async () => {
        if (!selectedLanguage) return;

        if (isLanguageMarkdown(selectedLanguage.name)) {
            currentPreviewContent = marked.parse(getContent(), { gfm: true }) as string;
        } else {
            const res = await fetch("/internal/highlight", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    content: getContent(),
                    language: selectedLanguage.name,
                    wrap: settings.textWrap,
                    theme: settings.theme,
                    showLineNumbers: true
                })
            });

            currentPreviewContent = await res.text();
        }
    };

    const onPreviewClick = async () => {
        await preview();

        previewEnabled = !previewEnabled;
    };

    export const focus = (): void => {
        editorView?.focus();
    };

    export const getContent = (): string => {
        return editorView.state.doc.toString();
    };

    export const getCursorPos = (): { line: number; col: number } => {
        return { line: cursorLine, col: cursorCol };
    };

    export const setCursorPos = (line: number, col: number) => {
        editorView.dispatch({
            selection: {
                anchor: editorView.state.doc.line(line).from + col,
                head: editorView.state.doc.line(line).from + col
            }
        });
    };

    export const setContent = (content: string) => {
        const transaction = editorView.state.update({
            changes: { from: 0, to: editorView.state.doc.length, insert: content }
        });

        editorView.update([transaction]);
    };

    export const getSelectedLang = (): Language | undefined => {
        return selectedLanguage;
    };

    export const setSelectedLang = (lang: Language) => {
        selectedLanguage = lang;

        const langDescription = cmLangs.find(
            (l) => selectedLanguage!.name.toLowerCase() === l.name.toLowerCase()
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
            if (["Text", "Autodetect"].includes(selectedLanguage.name)) {
                langSupported = true;
            }

            editorView.dispatch({
                effects: langCompartment.reconfigure([])
            });
        }

        if (previewEnabled) preview();
    };
</script>

<div class:hidden>
    {#if previewEnabled}
        <div class="preview" class:markdown={isLanguageMarkdown(selectedLanguage!.name)}>
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
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
            <button onclick={onLanguageClick}>language: {selectedLanguage?.name}</button>

            <button onclick={onIndentClick}>{selectedIndentUnit}: {selectedIndentWidth}</button>

            <button onclick={onPreviewClick} class:enabled={previewEnabled}>preview</button>
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
                background-color: var(--color-bg1);
                border-color: var(--color-bg3);

                :global(.cm-gutter),
                :global(.cm-activeLineGutter) {
                    background-color: var(--color-bg1);
                }
            }
        }

        :global(.cm-focused) {
            border-color: var(--color-primary) !important;
        }

        :global(.cm-scroller) {
            border-radius: $border-radius;
            line-height: 1.25rem;
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

        :global(pre) {
            margin-top: 0;
            padding-top: 0;
        }

        :global(code) {
            border: none;
            font-size: $fs-normal;
            padding: 0;
            border-radius: 0;
            background-color: transparent;
            display: flex;
            flex-direction: column;
            min-width: max-content;
            width: auto;
        }

        :global(code .line-number) {
            margin-right: 1rem;
            display: inline-block;
            text-align: right;
            color: var(--color-bg3);
            font-size: $fs-normal;
            user-select: none;
            @include transition();
        }

        &.markdown {
            padding: 0 1rem;
        }
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
