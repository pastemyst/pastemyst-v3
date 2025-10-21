---
sidebar_position: 1
slug: /themes
---

# Adding new themes to PasteMyst

This guide explains how to add new themes to PasteMyst.

When adding a new theme to PasteMyst you actually need to create three themes, one for PasteMyst itself, one for [CodeMirror](https://codemirror.net/) (the editor used for editing pastes) and one for [shiki](https://shiki.matsu.io/) (the library used to render final pastes).

You can of course use pre existing themes for CodeMirror and shiki, and create your own for PasteMyst (which isn't difficult).

## CodeMirror

There already exists quite a few CodeMirror themes you can add, but in the case that the theme you want to add to PasteMyst doesn't exist for CodeMirror yet, you will have to create your own. This is a bit more involved. You can take a look at the existing themes as a base, and also take a look at the [CodeMirror documentation](https://codemirror.net/examples/styling/).

To add a CodeMirror theme to PasteMyst add the theme file to the `client/src/lib/codemirror-themes` directory.

## Shiki

Shiki is a library that renders code with syntax highlighting while utilizing the already standard [TextMate grammars](https://macromates.com/manual/en/language_grammars). These grammar files are also in vscode, so you should be able to easily find the grammar file for the theme you want to add, or use one of the tools to create your own.

To add a shiki/textmate grammar to PasteMyst add the **JSON** grammar file (you can find a TextMate grammar to JSON compiler online, or convert it from inside vscode) to the `client/static/themes` directory.

## Connecting everything together

Finally you will have to create a theme for PasteMyst itself, and connect it all together.

First add the theme to the `client/src/app.scss` file like so:

```css
#theme-context[data-theme="dracula"] {
    --color-bg: #16171e;
    --color-bg1: #282a36;
    --color-bg2: #44475a;
    --color-bg3: #777777;

    --color-fg: #f8f8f2;

    --color-primary: #ffb86c;
    --color-secondary: #bd93f9;

    --color-danger: #ff5555;
    --color-success: #50fa7b;
    --color-pink: #ff79c6;
}
```

And then add the theme to the `themes` array in the `client/src/lib/themes.ts` file like so:

```ts
{
    name: "dracula",
    colors: {
        bg: "#16171e",
        bg1: "#282a36",
        bg2: "#44475a",
        bg3: "#777777",
        fg: "#F8F8F2",
        primary: "#FFB86C",
        secondary: "#BD93F9",
        danger: "#FF5555",
        success: "#50FA7B",
        pink: "#FF79C6"
    },
    codemirrorTheme: dracula,
    shikiTheme: "dracula"
},
```

The `codemirrorTheme` field is a reference to the previous CodeMirror theme you defined in the `client/src/lib/codemirror-themes` directory, and the `shikiTheme` field is the name of the TextMate grammar.

And that is all, your theme will now be available to be set in the PasteMyst settings, and you can create a [Pull Request](https://github.com/pastemyst/pastemyst-v3/pulls) to add the theme to the official PasteMyst instance.
