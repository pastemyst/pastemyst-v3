import type { Extension } from "@codemirror/state";
import { dracula } from "./codemirror-themes/dracula";
import { myst } from "./codemirror-themes/myst";
import { catppuccin } from "./codemirror-themes/catppuccin";
import { solarizedDark } from "./codemirror-themes/solarized";
import { rosePine } from "./codemirror-themes/rose-pine";
import { catppuccinLatte } from "./codemirror-themes/catppuccin-late";
import { rosePineDawn } from "./codemirror-themes/rose-pine-dawn";

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
    },
    {
        name: "catppuccin",
        colors: {
            bg: "#1e1e2e",
            bg1: "#313244",
            bg2: "#45475a",
            bg3: "#6c7086",
            fg: "#cdd6f4",
            primary: "#f5e0dc",
            secondary: "#cba6f7",
            danger: "#f38ba8",
            success: "#a6e3a1",
            pink: "#f5c2e7"
        },
        codemirrorTheme: catppuccin,
        shikiTheme: "catppuccin-mocha"
    },
    {
        name: "catppuccin-latte",
        colors: {
            bg: "#eff1f5",
            bg1: "#e6e9ef",
            bg2: "#ccd0da",
            bg3: "#acb0be",
            fg: "#4c4f69",
            primary: "#dc8a78",
            secondary: "#8839ef",
            danger: "#d20f39",
            success: "#40a02b",
            pink: "#ea76cb"
        },
        codemirrorTheme: catppuccinLatte,
        shikiTheme: "catppuccin-latte"
    },
    {
        name: "solarized",
        colors: {
            bg: "#002b36",
            bg1: "#073642",
            bg2: "#586e75",
            bg3: "#839496",
            fg: "#eee8d5",
            primary: "#268bd2",
            secondary: "#2aa198",
            danger: "#dc322f",
            success: "#859900",
            pink: "#d33682"
        },
        codemirrorTheme: solarizedDark,
        shikiTheme: "solarized-dark"
    },
    {
        name: "rose-pine",
        colors: {
            bg: "#191724",
            bg1: "#1f1d2e",
            bg2: "#26233a",
            bg3: "#6e6a86",
            fg: "#e0def4",
            primary: "#c4a7e7",
            secondary: "#31748f",
            danger: "#eb6f92",
            success: "#9ccfd8",
            pink: "#ebbcba"
        },
        codemirrorTheme: rosePine,
        shikiTheme: "rose-pine"
    },
    {
        name: "rose-pine-dawn",
        colors: {
            bg: "#faf4ed",
            bg1: "#fffaf3",
            bg2: "#f2e9e1",
            bg3: "#9893a5",
            fg: "#575279",
            primary: "#d7827e",
            secondary: "#286983",
            danger: "#b4637a",
            success: "#56949f",
            pink: "#907aa9"
        },
        codemirrorTheme: rosePineDawn,
        shikiTheme: "rose-pine-dawn"
    }
];
