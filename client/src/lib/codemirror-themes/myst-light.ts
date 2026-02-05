import { EditorView } from "@codemirror/view";
import type { Extension } from "@codemirror/state";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";

const bg = "#ffffff",
    cl = "#efefef",
    fg = "#4d4d4c",
    com = "#8e908c",
    red = "#c82829",
    orange = "#f5871f",
    green = "#718c00",
    aqua = "#3e999f",
    blue = "#4271ae",
    purple = "#8959a8";

const theme = EditorView.theme(
    {
        "&": {
            color: fg,
            backgroundColor: bg
        },

        ".cm-content": {
            caretColor: orange
        },

        "&.cm-focused .cm-cursor": {
            borderLeftColor: orange
        },

        "&.cm-focused .cm-selectionBackground, .cm-selectionBackground, ::selection": {
            backgroundColor: "#aafe6640"
        },

        ".cm-panels": {
            backgroundColor: bg,
            color: fg
        },

        ".cm-panels.cm-panels-top": {
            borderBottom: "2px solid black"
        },

        ".cm-panels.cm-panels-bottom": {
            borderTop: "2px solid black"
        },

        ".cm-searchMatch": {
            backgroundColor: "#72a1ff59",
            outline: "1px solid #457dff"
        },

        ".cm-searchMatch.cm-searchMatch-selected": {
            backgroundColor: "#6199ff2f"
        },

        ".cm-activeLine": {
            backgroundColor: cl,
            position: "relative",
            zIndex: -3
        },

        ".cm-selectionMatch": {
            backgroundColor: "#aafe661a"
        },

        ".cm-matchingBracket, .cm-nonmatchingBracket": {
            backgroundColor: "#bad0f847",
            outline: "1px solid #515a6b"
        },

        ".cm-gutters": {
            backgroundColor: bg,
            color: com,
            border: "none"
        },

        ".cm-activeLineGutter": {
            backgroundColor: bg
        },

        ".cm-foldPlaceholder": {
            backgroundColor: "transparent",
            border: "none",
            color: "#ddd"
        },

        ".cm-tooltip": {
            border: "1px solid #181a1f",
            backgroundColor: bg
        },
        ".cm-tooltip-autocomplete": {
            "& > ul > li[aria-selected]": {
                backgroundColor: bg,
                color: fg
            }
        }
    },
    { dark: false }
);

const highlightStyle = HighlightStyle.define([
    {
        tag: t.keyword,
        color: purple
    },

    {
        tag: [t.name, t.deleted, t.character, t.propertyName, t.macroName],
        color: red
    },

    {
        tag: [t.function(t.variableName), t.labelName],
        color: blue
    },

    {
        tag: [t.color, t.constant(t.name), t.standard(t.name)],
        color: orange
    },

    {
        tag: [t.definition(t.name), t.separator],
        color: fg
    },

    {
        tag: [
            t.typeName,
            t.className,
            t.number,
            t.changed,
            t.annotation,
            t.modifier,
            t.self,
            t.namespace
        ],
        color: orange
    },

    {
        tag: [
            t.operator,
            t.operatorKeyword,
            t.url,
            t.escape,
            t.regexp,
            t.link,
            t.special(t.string)
        ],
        color: aqua
    },

    {
        tag: [t.meta, t.comment],
        color: com
    },

    {
        tag: t.strong,
        fontWeight: "bold"
    },

    {
        tag: t.emphasis,
        fontStyle: "italic"
    },

    {
        tag: t.strikethrough,
        textDecoration: "line-through"
    },

    {
        tag: t.link,
        color: com,
        textDecoration: "underline"
    },

    {
        tag: t.heading,
        fontWeight: "bold",
        color: red
    },

    {
        tag: [t.atom, t.bool, t.special(t.variableName)],
        color: orange
    },

    {
        tag: [t.processingInstruction, t.string, t.inserted],
        color: green
    },

    {
        tag: t.invalid,
        color: "#000000"
    }
]);

export const mystLight: Extension = [theme, syntaxHighlighting(highlightStyle)];
