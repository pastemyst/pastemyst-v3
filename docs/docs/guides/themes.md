---
description: "How to add a new theme to PasteMyst: the site theme, the CodeMirror editor theme and the Shiki highlighting theme."
sidebar_position: 1
slug: /themes
---

# Adding new themes to PasteMyst

This guide explains how to add new themes to PasteMyst.

When adding a new theme to PasteMyst you actually need to create three themes, one for PasteMyst itself, one for [CodeMirror](https://codemirror.net/) (the editor used for editing pastes) and one for [shiki](https://shiki.matsu.io/) (the library used to render final pastes).

You can of course use pre-existing themes for CodeMirror and shiki, and create your own for PasteMyst (which isn't difficult).

## CodeMirror

There already exist quite a few CodeMirror themes you can add, but in the case that the theme you want to add to PasteMyst doesn't exist for CodeMirror yet, you will have to create your own. This is a bit more involved. You can take a look at the existing themes as a base, and also take a look at the [CodeMirror documentation](https://codemirror.net/examples/styling/).

To add a CodeMirror theme to PasteMyst add the theme file to the `client/src/lib/codemirror-themes` directory.

The CodeMirror theme is only used to change the looks of things outside the syntax highlighting, things like selections, gutters, etc. Syntax highlighting is rendered using shiki.

## Shiki

Shiki is a library that renders code with syntax highlighting. It uses the same colour themes as VS Code (TextMate-style themes in **JSON** format), so you should be able to easily find an existing VS Code theme for the theme you want to add, or create your own.

To add a shiki theme to PasteMyst, add the **JSON** theme file to the `client/static/themes` directory. The file must be named `<shikiTheme>.json`, where `<shikiTheme>` is the value of the `shikiTheme` field described below (for example `dracula.json`). PasteMyst loads it by that name, both in the editor and when rendering pastes.

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

Then import the CodeMirror theme at the top of the `client/src/lib/themes.ts` file:

```ts
import { dracula } from "./codemirror-themes/dracula";
```

And add the theme to the `themes` array in the same file like so:

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

The `codemirrorTheme` field is a reference to the previous CodeMirror theme you defined in the `client/src/lib/codemirror-themes` directory, and the `shikiTheme` field is the name of the shiki theme file in `client/static/themes`, without the `.json` extension.

And that is all, your theme will now be available to be set in the PasteMyst settings, and you can create a [Pull Request](https://github.com/pastemyst/pastemyst-v3/pulls) to add the theme to the official PasteMyst instance.
