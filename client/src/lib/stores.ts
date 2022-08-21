import { writable, type Writable } from "svelte/store";
import type { User } from "./api/user";
import type { Command } from "./command";

export const currentUserStore: Writable<User | null> = writable();

export const versionStore = writable("undefined");

export const activePastesStores = writable(0);

export const cmdPalOpen = writable(false);
export const cmdPalCommands: Writable<Command[]> = writable([]);
