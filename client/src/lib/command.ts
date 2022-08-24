export interface Command {
    name: string;
    description?: string;
    action: CommandAction;
}

export type CommandAction = () => void;
