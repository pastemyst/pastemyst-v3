import { apiBase } from "./api";

export interface Release {
    url: string;
    title: string;
    content: string;
    isPrerelease: boolean;
    releasedAt: string;
}

export const getVersion = async (): Promise<string> => {
    const res = await fetch(`${apiBase}/meta/version`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).version;

    return "unknown version";
};

export const getReleases = async(): Promise<Release[]> => {
    const res = await fetch(`${apiBase}/meta/releases`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).releases;

    return [];
};

export const getActivePastes = async (): Promise<number> => {
    const res = await fetch(`${apiBase}/meta/active_pastes`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).count;

    return 0;
};
