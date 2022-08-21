import { apiBase } from "./api";
import type { FetchFunc } from "./fetch";

export interface Release {
    url: string;
    title: string;
    content: string;
    isPrerelease: boolean;
    releasedAt: string;
}

export const getVersion = async (fetchFunc: FetchFunc): Promise<string> => {
    const res = await fetchFunc(`${apiBase}/meta/version`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).version;

    return "unknown version";
};

export const getReleases = async (fetchFunc: FetchFunc): Promise<Release[]> => {
    const res = await fetchFunc(`${apiBase}/meta/releases`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).releases;

    return [];
};

export const getActivePastes = async (fetchFunc: FetchFunc): Promise<number> => {
    const res = await fetchFunc(`${apiBase}/meta/active_pastes`, {
        method: "get"
    });

    if (res.ok) return (await res.json()).count;

    return 0;
};
