import { writable, type Writable } from "svelte/store";
import type { User } from "./api/user";
import { themes } from "./themes";

export const currentUserStore: Writable<User | null> = writable();

export const versionStore = writable("undefined");

export const activePastesStores = writable(0);

export const cmdPalOpen = writable(false);

export const themeStore = writable(themes[1]);
