import type { Extension } from "@codemirror/state";
import { dracula } from "./codemirror-themes/dracula";
import { myst } from "./codemirror-themes/myst";

export interface Theme {
    name: string;
    colors: ThemeColors;
    codemirrorTheme: Extension;
    shikiTheme: string;
}

export interface ThemeColors {
    bg: string;
    bg1: string;
    bg2: string;
    bg3: string;

    fg: string;

    primary: string;
    secondary: string;

    danger: string;
    success: string;
    pink: string;
}

export const themes: Theme[] = [
    {
        name: "myst",
        colors: {
            bg: "#141414",
            bg1: "#1c1c1c",
            bg2: "#222222",
            bg3: "#777777",
            fg: "#cccccc",
            primary: "#ee720d",
            secondary: "#1eaedb",
            danger: "#ff4c4c",
            success: "#2ec933",
            pink: "#cb6dce"
        },
        codemirrorTheme: myst,
        shikiTheme: "tomorrowmyst"
    },
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
    }
];
