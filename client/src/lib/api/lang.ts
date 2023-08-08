import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";

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

export const getLangs = async (fetchFunc: FetchFunc): Promise<Language[]> => {
    if (langs.length === 0) {
        const res = await fetchFunc(`${env.PUBLIC_API_BASE}/langs`);

        if (res.ok) langs = await res.json();
    }

    return langs;
};

export const getPopularLangNames = async (fetchFunc: FetchFunc): Promise<string[]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/langs/popular`);

    if (res.ok) return await res.json();

    return [];
};
