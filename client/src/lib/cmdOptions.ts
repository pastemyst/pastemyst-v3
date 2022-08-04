import { getLangs } from "./api/lang";

export class Command {
    name: string;
    icon?: string = undefined;
    description?: string = undefined;
    shortcuts: string[][] = [];

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
    url!: string;

    constructor(name: string) {
        super(name);
    }

    withUrl(url: string) {
        this.url = url;
        return this;
    }
}

export class DirCommand extends Command {
    subCommands: Command[] = [];

    constructor(name: string) {
        super(name);
    }

    withCommands(commands: Command[]) {
        this.subCommands = commands;
        return this;
    }
}

export class SelectCommand extends Command {
    options: SelectOptionCommand[] = [];

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

    setSelected(name: string) {
        const selected = this.getSelected();
        if (selected) {
            selected.selected = false;
        }

        for (const opt of this.options) {
            if (name.toLowerCase() === opt.name.toLowerCase()) {
                opt.selected = true;
            }
        }
    }
}

export class SelectOptionCommand extends Command {
    selected: boolean;
    parent!: SelectCommand;

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

export const langSelect = new SelectCommand("set language")
    .withDescription("set the language of the currently active editor")
    .withIcon(
        `<svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512"><title>Language</title><path fill="currentColor" d="M478.33 433.6l-90-218a22 22 0 00-40.67 0l-90 218a22 22 0 1040.67 16.79L316.66 406h102.67l18.33 44.39A22 22 0 00458 464a22 22 0 0020.32-30.4zM334.83 362L368 281.65 401.17 362zM267.84 342.92a22 22 0 00-4.89-30.7c-.2-.15-15-11.13-36.49-34.73 39.65-53.68 62.11-114.75 71.27-143.49H330a22 22 0 000-44H214V70a22 22 0 00-44 0v20H54a22 22 0 000 44h197.25c-9.52 26.95-27.05 69.5-53.79 108.36-31.41-41.68-43.08-68.65-43.17-68.87a22 22 0 00-40.58 17c.58 1.38 14.55 34.23 52.86 83.93.92 1.19 1.83 2.35 2.74 3.51-39.24 44.35-77.74 71.86-93.85 80.74a22 22 0 1021.07 38.63c2.16-1.18 48.6-26.89 101.63-85.59 22.52 24.08 38 35.44 38.93 36.1a22 22 0 0030.75-4.9z"/></svg>`
    );

(async () => {
    const langOptions: SelectOptionCommand[] = [];
    const langs = await getLangs();
    for (let i = 0; i < langs.length; i++) {
        langOptions.push(
            new SelectOptionCommand(langs[i].name, i === 0).withDescription(
                langs[i].aliases?.join(", ")
            )
        );
    }

    langSelect.withOptions(langOptions);
})();

export const indentSelect = new DirCommand("set indentation")
    .withDescription("set the indentation unit and width of the currently active editor")
    .withIcon(
        `<svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512"><title>Arrow Forward</title><path fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="48" d="M268 112l144 144-144 144M392 256H100"/></svg>`
    )
    .withCommands([
        new SelectCommand("tabs")
            .withDescription("ident using tabs")
            .withOptions([
                new SelectOptionCommand("1"),
                new SelectOptionCommand("2"),
                new SelectOptionCommand("3"),
                new SelectOptionCommand("4"),
                new SelectOptionCommand("5"),
                new SelectOptionCommand("6"),
                new SelectOptionCommand("7"),
                new SelectOptionCommand("8")
            ]),
        new SelectCommand("spaces")
            .withDescription("ident using spaces")
            .withOptions([
                new SelectOptionCommand("1"),
                new SelectOptionCommand("2"),
                new SelectOptionCommand("3"),
                new SelectOptionCommand("4", true),
                new SelectOptionCommand("5"),
                new SelectOptionCommand("6"),
                new SelectOptionCommand("7"),
                new SelectOptionCommand("8")
            ])
    ]);

export const rootCommands: Command[] = [expiresSelect, langSelect, indentSelect];
