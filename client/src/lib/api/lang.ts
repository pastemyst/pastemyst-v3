import { apiBase } from "./api";

export interface Language {
    name: string,
    type: string,
    aliases: string[],
    codemirrorMode: string,
    codemirrorMimeType: string,
    wrap: boolean,
    extensions: string[],
    color: string,
    tmScope: string
}

export const getLangs = async (): Promise<Language[]> => {
    const res = await fetch(`${apiBase}/lang/all`);

    if (res.ok) return await res.json();

    return [];
};

export const langs: Language[] = await getLangs();
