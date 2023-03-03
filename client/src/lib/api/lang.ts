import { PUBLIC_API_BASE } from "$env/static/public";

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

export interface LangStat {
    language: Language;
    percentage: number;
}

let langs: Language[] = [];

export const getLangs = async (): Promise<Language[]> => {
    if (langs.length === 0) {
        const res = await fetch(`${PUBLIC_API_BASE}/langs`);

        if (res.ok) langs = await res.json();
    }

    return langs;
};
