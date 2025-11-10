import { writable, type Writable } from "svelte/store";

export interface Command {
    name: string;
    description?: string;
    action: CommandAction;
    onHover?: CommandHoverAction;
    onClose?: CommandPaletteCloseAction;
}

export enum Close {
    yes,
    no
}

export type CommandAction = () => Close;
export type CommandHoverAction = () => void;
export type CommandPaletteCloseAction = () => void;

let baseCommands: Command[] = [];

export const baseCommandsStore: Writable<Command[]> = writable(baseCommands);
export const tempCommandsStore: Writable<Command[]> = writable([]);
export const preselectedCommandStore: Writable<string | undefined> = writable();

export const getBaseCommands = (): Command[] => {
    return baseCommands;
};

export const setBaseCommands = (commands: Command[]) => {
    baseCommands = commands;

    baseCommandsStore.set(baseCommands);
};

export const addBaseCommands = (commands: Command[]) => {
    baseCommands.unshift(...commands);

    baseCommandsStore.set(baseCommands);
};

export const setTempCommands = (commands: Command[]) => {
    tempCommandsStore.set(commands);
};

export const setPreselectedCommand = (command: string) => {
    preselectedCommandStore.set(command);
};

export const getConfirmActionCommands = (confirmAction: CommandAction): Command[] => [
    {
        name: "yes",
        action: confirmAction
    },
    {
        name: "no",
        action: () => Close.yes
    }
];
