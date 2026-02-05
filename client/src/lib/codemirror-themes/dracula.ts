import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import type { Extension } from "@codemirror/state";
import { EditorView } from "@codemirror/view";
import { tags as t } from "@lezer/highlight";

// Author: Zeno Rocha

const background = "#2d2f3f";
const foreground = "#f8f8f2";
const caret = "#f8f8f0";
const gutterBackground = "#282a36";
const gutterForeground = "rgb(144, 145, 148)";
const lineHighlight = "#44475a";

const theme = EditorView.theme(
    {
        "&": {
            backgroundColor: background,
            color: foreground
        },
        ".cm-content": {
            caretColor: caret
        },
        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: caret
        },
        "&.cm-focused .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection": {
            backgroundColor: "#99ff7780"
        },
        ".cm-activeLine": {
            backgroundColor: lineHighlight,
            position: "relative",
            zIndex: -3
        },
        ".cm-gutters": {
            backgroundColor: gutterBackground,
            color: gutterForeground
        },
        ".cm-activeLineGutter": {
            backgroundColor: lineHighlight
        }
    },
    {
        dark: true
    }
);

const highlightStyle = HighlightStyle.define([
    {
        tag: t.comment,
        color: "#6272a4"
    },
    {
        tag: [t.string, t.special(t.brace)],
        color: "#f1fa8c"
    },
    {
        tag: [t.number, t.self, t.bool, t.null],
        color: "#bd93f9"
    },
    {
        tag: [t.keyword, t.operator],
        color: "#ff79c6"
    },
    {
        tag: [t.definitionKeyword, t.typeName],
        color: "#8be9fd"
    },
    {
        tag: t.definition(t.typeName),
        color: "#f8f8f2"
    },
    {
        tag: [
            t.className,
            t.definition(t.propertyName),
            t.function(t.variableName),
            t.attributeName
        ],
        color: "#50fa7b"
    }
]);

export const dracula: Extension = [theme, syntaxHighlighting(highlightStyle)];
