import { API_URL, type FetchFunc } from "./fetch";

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
        const res = await fetchFunc(`${API_URL}/langs`);

        if (res.ok) langs = await res.json();
    }

    return langs;
};

export const getPopularLangNames = async (fetchFunc: FetchFunc): Promise<string[]> => {
    const res = await fetchFunc(`${API_URL}/langs/popular`);

    if (res.ok) return await res.json();

    return [];
};

export const autodetectLanguage = async (content: string): Promise<Language> => {
    const res = await fetch(`${API_URL}/langs/autodetect`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(content)
    });

    if (res.ok) return await res.json();

    return (await getLangs(fetch)).find((lang) => lang.name === "Text")!;
};

export const findLangByName = async (
    fetchFunc: FetchFunc,
    lang: string
): Promise<Language | undefined> => {
    const res = await fetchFunc(`${API_URL}/langs/${encodeURIComponent(lang)}`);

    if (res.ok) return await res.json();

    return undefined;
};
