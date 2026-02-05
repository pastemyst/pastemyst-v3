import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import type { Extension } from "@codemirror/state";
import { tags as t } from "@lezer/highlight";
import { EditorView } from "codemirror";

const theme = EditorView.theme(
    {
        "&": {
            color: "#cdd6f4",
            backgroundColor: "#1e1e2e"
        },

        ".cm-content": {
            caretColor: "#f5e0dc"
        },

        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: "#f5e0dc"
        },

        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground": {
            backgroundColor: "#585b7080"
        },

        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection":
            {
                backgroundColor: "#585b7080"
            },

        ".cm-panels": {
            backgroundColor: "#181825",
            color: "#cdd6f4"
        },
        ".cm-panels.cm-panels-top": { borderBottom: "2px solid black" },
        ".cm-panels.cm-panels-bottom": { borderTop: "2px solid black" },

        ".cm-searchMatch": {
            backgroundColor: `${"#89b4fa"}59`,
            outline: `1px solid ${"#89b4fa"}`
        },
        ".cm-searchMatch.cm-searchMatch-selected": {
            backgroundColor: `${"#89b4fa"}2f`
        },

        ".cm-activeLine": {
            backgroundColor: "#313244",
            position: "relative",
            zIndex: -3
        },
        ".cm-selectionMatch": {
            backgroundColor: `${"#585b70"}4d`
        },

        "&.cm-focused .cm-matchingBracket, &.cm-focused .cm-nonmatchingBracket": {
            backgroundColor: `${"#585b70"}47`,
            color: "#cdd6f4"
        },

        ".cm-gutters": {
            backgroundColor: "#1e1e2e",
            color: "#a6adc8",
            border: "none"
        },

        ".cm-activeLineGutter": {
            backgroundColor: "#313244"
        },

        ".cm-foldPlaceholder": {
            backgroundColor: "transparent",
            border: "none",
            color: "#6c7086"
        },

        ".cm-tooltip": {
            border: "none",
            backgroundColor: "#313244"
        },
        ".cm-tooltip .cm-tooltip-arrow:before": {
            borderTopColor: "transparent",
            borderBottomColor: "transparent"
        },
        ".cm-tooltip .cm-tooltip-arrow:after": {
            borderTopColor: "#313244",
            borderBottomColor: "#313244"
        },
        ".cm-tooltip-autocomplete": {
            "& > ul > li[aria-selected]": {
                backgroundColor: "#45475a",
                color: "#cdd6f4"
            }
        }
    },
    { dark: true }
);

const highlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: "#cba6f7" },
    {
        tag: [t.name, t.definition(t.name), t.deleted, t.character, t.macroName],
        color: "#cdd6f4"
    },
    {
        tag: [t.function(t.variableName), t.function(t.propertyName), t.propertyName, t.labelName],
        color: "#89b4fa"
    },
    {
        tag: [t.color, t.constant(t.name), t.standard(t.name)],
        color: "#fab387"
    },
    { tag: [t.self, t.atom], color: "#f38ba8" },
    {
        tag: [t.typeName, t.className, t.changed, t.annotation, t.namespace],
        color: "#f9e2af"
    },
    { tag: [t.operator], color: "#89dceb" },
    { tag: [t.url, t.link], color: "#94e2d5" },
    { tag: [t.escape, t.regexp], color: "#f5c2e7" },
    {
        tag: [t.meta, t.punctuation, t.separator, t.comment],
        color: "#9399b2"
    },
    { tag: t.strong, fontWeight: "bold" },
    { tag: t.emphasis, fontStyle: "italic" },
    { tag: t.strikethrough, textDecoration: "line-through" },
    { tag: t.link, color: "#89b4fa", textDecoration: "underline" },
    { tag: t.heading, fontWeight: "bold", color: "#89b4fa" },
    {
        tag: [t.special(t.variableName)],
        color: "#b4befe"
    },
    { tag: [t.bool, t.number], color: "#fab387" },
    {
        tag: [t.processingInstruction, t.string, t.inserted],
        color: "#a6e3a1"
    },
    { tag: t.invalid, color: "#f38ba8" }
]);

export const catppuccin: Extension = [theme, syntaxHighlighting(highlightStyle)];
