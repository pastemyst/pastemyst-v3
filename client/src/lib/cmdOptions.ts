export class Command {
    name: string;
    icon?: string = null;
    description?: string = null;
    shortcuts?: string[][] = [];

    constructor(name: string) {
        this.name = name;
    }

    withName(name: string) {
        this.name = name;
        return this;
    }

    withIcon(icon: string) {
        this.icon = icon;
        return this;
    }

    withDescription(description: string) {
        this.description = description;
        return this;
    }

    addShortcut(shortcut: string[]) {
        this.shortcuts.push(shortcut);
        return this;
    }
}

export class LinkCommand extends Command {
    url: string;

    constructor(name: string) {
        super(name);
    }

    withUrl(url: string) {
        this.url = url;
        return this;
    }
}

export class DirCommand extends Command {
    subCommands: Command[];

    constructor(name: string) {
        super(name);
    }

    withCommands(commands: Command[]) {
        this.subCommands = commands;
        return this;
    }
}

export class SelectCommand extends Command {
    options: SelectOptionCommand[];

    constructor(name: string) {
        super(name);
    }

    withOptions(options: SelectOptionCommand[]) {
        this.options = options;

        for (const opt of options) {
            opt.parent = this;
        }

        return this;
    }

    getSelected() {
        for (const opt of this.options) {
            if (opt.selected) return opt;
        }
    }
}

export class SelectOptionCommand extends Command {
    selected: boolean;
    parent: SelectCommand;

    constructor(name: string, selected = false) {
        super(name);
        this.selected = selected;
    }
}

export const expiresSelect = new SelectCommand("set expires in")
    .withOptions([
        new SelectOptionCommand("never", true),
        new SelectOptionCommand("1 hour"),
        new SelectOptionCommand("2 hours"),
        new SelectOptionCommand("10 hours"),
        new SelectOptionCommand("1 day"),
        new SelectOptionCommand("2 days"),
        new SelectOptionCommand("1 week"),
        new SelectOptionCommand("1 year")
    ])
    .withDescription("set when the paste will expire")
    .addShortcut(["ctrl", "e"])
    .withIcon(
        `<svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512"><title>Time</title><path fill="currentColor" d="M256 48C141.13 48 48 141.13 48 256s93.13 208 208 208 208-93.13 208-208S370.87 48 256 48zm96 240h-96a16 16 0 01-16-16V128a16 16 0 0132 0v128h80a16 16 0 010 32z"/></svg>`
    );

export const rootCommands: Command[] = [expiresSelect];
