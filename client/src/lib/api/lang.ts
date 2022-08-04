import { apiBase } from "./api";

export interface Language {
    name: string;
    type: string;
    aliases: string[];
    codemirrorMode: string;
    codemirrorMimeType: string;
    wrap: boolean;
    extensions: string[];
    color: string;
    tmScope: string;
}

let langs: Language[] = [];

export const getLangs = async (): Promise<Language[]> => {
    if (langs.length === 0) {
        const res = await fetch(`${apiBase}/lang/all`);

        if (res.ok) langs = await res.json();
    }

    return langs;
};
