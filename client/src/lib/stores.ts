import { writable, type Writable } from "svelte/store";
import type { User } from "./api/user";
import { themes } from "./themes";
import { type Settings, type SettingsContext } from "./api/settings";

export const currentUserStore: Writable<User | null> = writable();

export const versionStore = writable("undefined");

export const activePastesStores = writable(0);

export const cmdPalOpen = writable(false);
export const cmdPalTitle = writable<string | null>(null);

export const themeStore = writable(themes[0]);

export const creatingPasteStore = writable(false);

export const settingsContextStore = writable<SettingsContext>("browser");
export const settingsStore = writable<Settings>();
