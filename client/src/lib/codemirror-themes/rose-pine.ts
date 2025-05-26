import { EditorView } from "@codemirror/view";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";
import type { Extension } from "@codemirror/state";

const theme = EditorView.theme(
    {
        "&": {
            color: "#e0def4",
            backgroundColor: "#191724"
        },

        ".cm-content": {
            caretColor: "#ebbcba"
        },

        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: "#ebbcba"
        },

        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection":
            {
                backgroundColor: "#26233a"
            },

        ".cm-panels": {
            backgroundColor: "#6e6a86",
            color: "#e0def4"
        },
        ".cm-panels.cm-panels-top": { borderBottom: "2px solid black" },
        ".cm-panels.cm-panels-bottom": { borderTop: "2px solid black" },

        ".cm-searchMatch": {
            backgroundColor: `#9ccfd859`,
            outline: `1px solid #9ccfd8`
        },
        ".cm-searchMatch.cm-searchMatch-selected": {
            backgroundColor: `#9ccfd82f`
        },

        ".cm-activeLine": { backgroundColor: "#1f1d2e" },
        ".cm-selectionMatch": {
            backgroundColor: `#6e6a864d`
        },

        "&.cm-focused .cm-matchingBracket, &.cm-focused .cm-nonmatchingBracket": {
            backgroundColor: `#6e6a8647`,
            color: "#e0def4"
        },

        ".cm-gutters": {
            backgroundColor: "#191724",
            color: "#908caa",
            border: "none"
        },

        ".cm-activeLineGutter": {
            backgroundColor: "#1f1d2e"
        },

        ".cm-foldPlaceholder": {
            backgroundColor: "transparent",
            border: "none",
            color: "#26233a"
        },

        ".cm-tooltip": {
            border: "none",
            backgroundColor: "#1f1d2e"
        },
        ".cm-tooltip .cm-tooltip-arrow:before": {
            borderTopColor: "transparent",
            borderBottomColor: "transparent"
        },
        ".cm-tooltip .cm-tooltip-arrow:after": {
            borderTopColor: "#1f1d2e",
            borderBottomColor: "#1f1d2e"
        },
        ".cm-tooltip-autocomplete": {
            "& > ul > li[aria-selected]": {
                backgroundColor: "#26233a",
                color: "#e0def4"
            }
        }
    },
    { dark: true }
);

const highlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: "#c4a7e7" },
    {
        tag: [t.name, t.definition(t.name), t.deleted, t.character, t.macroName],
        color: "#e0def4"
    },
    {
        tag: [t.function(t.variableName), t.function(t.propertyName), t.propertyName, t.labelName],
        color: "#9ccfd8"
    },
    {
        tag: [t.color, t.constant(t.name), t.standard(t.name)],
        color: "#f6c177"
    },
    { tag: [t.self, t.atom], color: "#eb6f92" },
    {
        tag: [t.typeName, t.className, t.changed, t.annotation, t.namespace],
        color: "#f6c177"
    },
    { tag: [t.operator], color: "#9ccfd8" },
    { tag: [t.url, t.link], color: "#c4a7e7" },
    { tag: [t.escape, t.regexp], color: "#ebbcba" },
    {
        tag: [t.meta, t.punctuation, t.separator, t.comment],
        color: "#908caa"
    },
    { tag: t.strong, fontWeight: "bold" },
    { tag: t.emphasis, fontStyle: "italic" },
    { tag: t.strikethrough, textDecoration: "line-through" },
    { tag: t.link, color: "#9ccfd8", textDecoration: "underline" },
    { tag: t.heading, fontWeight: "bold", color: "#9ccfd8" },
    {
        tag: [t.special(t.variableName)],
        color: "#c4a7e7"
    },
    { tag: [t.bool, t.number], color: "#ebbcba" },
    {
        tag: [t.processingInstruction, t.string, t.inserted],
        color: "#9ccfd8"
    },
    { tag: t.invalid, color: "#eb6f92" }
]);

export const rosePine: Extension = [theme, syntaxHighlighting(highlightStyle)];
