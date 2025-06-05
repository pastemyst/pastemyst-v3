import { readable, writable, type Writable } from "svelte/store";
import type { User } from "./api/user";
import { type Theme } from "./themes";
import { browser } from "$app/environment";
import { type Settings } from "./api/settings";

export const currentUserStore: Writable<User | null> = writable();

export const versionStore = writable("undefined");

export const activePastesStores = writable(0);

export const cmdPalOpen = writable(false);
export const cmdPalTitle = writable<string | null>(null);

export const themeStore = writable<Theme | null>(null);

export const creatingPasteStore = writable(false);

export const copyLinkToClipboardStore = writable<boolean>(false);

export const settingsStore = $state<{ settings: Settings | null }>({ settings: null });

const getSystemTheme = () => {
    if (!browser) return "dark";

    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
};

export const systemThemeStore = readable(getSystemTheme(), (set) => {
    if (!browser) return;

    const media = window.matchMedia("(prefers-color-scheme: dark)");

    const update = () => {
        set(media.matches ? "dark" : "light");
    };

    media.addEventListener("change", update);

    return () => media.removeEventListener("change", update);
});
