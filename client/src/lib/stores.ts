import { writable, type Writable } from "svelte/store";
import type { User } from "./api/user";
import { type Theme } from "./themes";

export const currentUserStore: Writable<User | null> = writable();

export const versionStore = writable("undefined");

export const activePastesStores = writable(0);

export const cmdPalOpen = writable(false);
export const cmdPalTitle = writable<string | null>(null);

export const themeStore = writable<Theme | null>(null);

export const creatingPasteStore = writable(false);

export const copyLinkToClipboardStore = writable<boolean>(false);
