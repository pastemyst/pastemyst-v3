export interface Command {
    name: string;
    action: CommandAction;
}

export type CommandAction = () => void;
