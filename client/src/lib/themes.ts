import type { Extension } from "@codemirror/state";
import { dracula } from "./codemirror-themes/dracula";
import { myst } from "./codemirror-themes/myst";
import { catppuccin } from "./codemirror-themes/catppuccin";
import { solarizedDark } from "./codemirror-themes/solarized";
import { rosePine } from "./codemirror-themes/rose-pine";

export interface Theme {
    name: string;
    codemirrorTheme: Extension;
    shikiTheme: string;
}

export const themes: Theme[] = [
    {
        name: "myst",
        codemirrorTheme: myst,
        shikiTheme: "tomorrowmyst"
    },
    {
        name: "dracula",
        codemirrorTheme: dracula,
        shikiTheme: "dracula"
    },
    {
        name: "catppuccin",
        codemirrorTheme: catppuccin,
        shikiTheme: "catppuccin-mocha"
    },
    {
        name: "solarized",
        codemirrorTheme: solarizedDark,
        shikiTheme: "solarized-dark"
    },
    {
        name: "rose-pine",
        codemirrorTheme: rosePine,
        shikiTheme: "rose-pine"
    }
];
