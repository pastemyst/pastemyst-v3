import { EditorView } from "@codemirror/view";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";
import type { Extension } from "@codemirror/state";

const theme = EditorView.theme(
    {
        "&": {
            color: "#4c4f69",
            backgroundColor: "#eff1f5"
        },

        ".cm-content": {
            caretColor: "#dc8a78"
        },

        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: "#dc8a78"
        },

        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection":
            {
                backgroundColor: "#acb0be"
            },

        ".cm-panels": {
            backgroundColor: "#e6e9ef",
            color: "#4c4f69"
        },
        ".cm-panels.cm-panels-top": { borderBottom: "2px solid black" },
        ".cm-panels.cm-panels-bottom": { borderTop: "2px solid black" },

        ".cm-searchMatch": {
            backgroundColor: `${"#1e66f5"}59`,
            outline: `1px solid ${"#1e66f5"}`
        },
        ".cm-searchMatch.cm-searchMatch-selected": {
            backgroundColor: `${"#1e66f5"}2f`
        },

        ".cm-activeLine": { backgroundColor: "#ccd0da" },
        ".cm-selectionMatch": {
            backgroundColor: `${"#acb0be"}4d`
        },

        "&.cm-focused .cm-matchingBracket, &.cm-focused .cm-nonmatchingBracket": {
            backgroundColor: `${"#acb0be"}47`,
            color: "#4c4f69"
        },

        ".cm-gutters": {
            backgroundColor: "#eff1f5",
            color: "#6c6f85",
            border: "none"
        },

        ".cm-activeLineGutter": {
            backgroundColor: "#ccd0da"
        },

        ".cm-foldPlaceholder": {
            backgroundColor: "transparent",
            border: "none",
            color: "#9ca0b0"
        },

        ".cm-tooltip": {
            border: "none",
            backgroundColor: "#ccd0da"
        },
        ".cm-tooltip .cm-tooltip-arrow:before": {
            borderTopColor: "transparent",
            borderBottomColor: "transparent"
        },
        ".cm-tooltip .cm-tooltip-arrow:after": {
            borderTopColor: "#ccd0da",
            borderBottomColor: "#ccd0da"
        },
        ".cm-tooltip-autocomplete": {
            "& > ul > li[aria-selected]": {
                backgroundColor: "#bcc0cc",
                color: "#4c4f69"
            }
        }
    },
    { dark: false }
);

const highlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: "#8839ef" },
    {
        tag: [t.name, t.definition(t.name), t.deleted, t.character, t.macroName],
        color: "#4c4f69"
    },
    {
        tag: [t.function(t.variableName), t.function(t.propertyName), t.propertyName, t.labelName],
        color: "#1e66f5"
    },
    {
        tag: [t.color, t.constant(t.name), t.standard(t.name)],
        color: "#fe640b"
    },
    { tag: [t.self, t.atom], color: "#d20f39" },
    {
        tag: [t.typeName, t.className, t.changed, t.annotation, t.namespace],
        color: "#df8e1d"
    },
    { tag: [t.operator], color: "#04a5e5" },
    { tag: [t.url, t.link], color: "#179299" },
    { tag: [t.escape, t.regexp], color: "#ea76cb" },
    {
        tag: [t.meta, t.punctuation, t.separator, t.comment],
        color: "#7c7f93"
    },
    { tag: t.strong, fontWeight: "bold" },
    { tag: t.emphasis, fontStyle: "italic" },
    { tag: t.strikethrough, textDecoration: "line-through" },
    { tag: t.link, color: "#1e66f5", textDecoration: "underline" },
    { tag: t.heading, fontWeight: "bold", color: "#1e66f5" },
    {
        tag: [t.special(t.variableName)],
        color: "#7287fd"
    },
    { tag: [t.bool, t.number], color: "#fe640b" },
    {
        tag: [t.processingInstruction, t.string, t.inserted],
        color: "#40a02b"
    },
    { tag: t.invalid, color: "#d20f39" }
]);

export const catppuccinLatte: Extension = [theme, syntaxHighlighting(highlightStyle)];
