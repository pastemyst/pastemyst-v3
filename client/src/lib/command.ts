import { writable, type Writable } from "svelte/store";

export interface Command {
    name: string;
    description?: string;
    action: CommandAction;
}

export type CommandAction = () => void;

let baseCommands: Command[] = [];

export const baseCommandsStore: Writable<Command[]> = writable(baseCommands);
export const tempCommandsStore: Writable<Command[]> = writable([]);

export const getBaseCommands = (): Command[] => {
    return baseCommands;
};

export const setBaseCommands = (commands: Command[]) => {
    baseCommands = commands;

    baseCommandsStore.set(baseCommands);
};

export const addBaseCommands = (commands: Command[]) => {
    baseCommands.push(...commands);

    baseCommandsStore.set(baseCommands);
};

export const setTempCommands = (commands: Command[]) => {
    tempCommandsStore.set(commands);
};
