import { EditorView } from "@codemirror/view";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";
import type { Extension } from "@codemirror/state";

const theme = EditorView.theme(
    {
        "&": {
            color: "#575279",
            backgroundColor: "#faf4ed"
        },

        ".cm-content": {
            caretColor: "#d7827e"
        },

        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: "#d7827e"
        },

        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection":
            {
                backgroundColor: "#f2e9e1"
            },

        ".cm-panels": {
            backgroundColor: "#9893a5",
            color: "#575279"
        },
        ".cm-panels.cm-panels-top": { borderBottom: "2px solid black" },
        ".cm-panels.cm-panels-bottom": { borderTop: "2px solid black" },

        ".cm-searchMatch": {
            backgroundColor: `#56949f59`,
            outline: `1px solid #56949f`
        },
        ".cm-searchMatch.cm-searchMatch-selected": {
            backgroundColor: `#56949f2f`
        },

        ".cm-activeLine": { backgroundColor: "#fffaf3" },
        ".cm-selectionMatch": {
            backgroundColor: `#9893a54d`
        },

        "&.cm-focused .cm-matchingBracket, &.cm-focused .cm-nonmatchingBracket": {
            backgroundColor: `#9893a547`,
            color: "#575279"
        },

        ".cm-gutters": {
            backgroundColor: "#faf4ed",
            color: "#797593",
            border: "none"
        },

        ".cm-activeLineGutter": {
            backgroundColor: "#fffaf3"
        },

        ".cm-foldPlaceholder": {
            backgroundColor: "transparent",
            border: "none",
            color: "#f2e9e1"
        },

        ".cm-tooltip": {
            border: "none",
            backgroundColor: "#fffaf3"
        },
        ".cm-tooltip .cm-tooltip-arrow:before": {
            borderTopColor: "transparent",
            borderBottomColor: "transparent"
        },
        ".cm-tooltip .cm-tooltip-arrow:after": {
            borderTopColor: "#fffaf3",
            borderBottomColor: "#fffaf3"
        },
        ".cm-tooltip-autocomplete": {
            "& > ul > li[aria-selected]": {
                backgroundColor: "#f2e9e1",
                color: "#575279"
            }
        }
    },
    { dark: true }
);

const highlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: "#907aa9" },
    {
        tag: [t.name, t.definition(t.name), t.deleted, t.character, t.macroName],
        color: "#575279"
    },
    {
        tag: [t.function(t.variableName), t.function(t.propertyName), t.propertyName, t.labelName],
        color: "#56949f"
    },
    {
        tag: [t.color, t.constant(t.name), t.standard(t.name)],
        color: "#ea9d34"
    },
    { tag: [t.self, t.atom], color: "#b4637a" },
    {
        tag: [t.typeName, t.className, t.changed, t.annotation, t.namespace],
        color: "#ea9d34"
    },
    { tag: [t.operator], color: "#56949f" },
    { tag: [t.url, t.link], color: "#907aa9" },
    { tag: [t.escape, t.regexp], color: "#d7827e" },
    {
        tag: [t.meta, t.punctuation, t.separator, t.comment],
        color: "#797593"
    },
    { tag: t.strong, fontWeight: "bold" },
    { tag: t.emphasis, fontStyle: "italic" },
    { tag: t.strikethrough, textDecoration: "line-through" },
    { tag: t.link, color: "#56949f", textDecoration: "underline" },
    { tag: t.heading, fontWeight: "bold", color: "#56949f" },
    {
        tag: [t.special(t.variableName)],
        color: "#907aa9"
    },
    { tag: [t.bool, t.number], color: "#d7827e" },
    {
        tag: [t.processingInstruction, t.string, t.inserted],
        color: "#56949f"
    },
    { tag: t.invalid, color: "#b4637a" }
]);

export const rosePineDawn: Extension = [theme, syntaxHighlighting(highlightStyle)];
