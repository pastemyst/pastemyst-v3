import { writable } from "svelte/store";

export const isCommandPaletteOpen = writable(false);
export const currentUserStore = writable(null);