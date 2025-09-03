import { getApiUrl, type FetchFunc } from "./fetch";

export interface Release {
    url: string;
    title: string;
    content: string;
    isPrerelease: boolean;
    releasedAt: string;
}

export interface AppStats {
    activePastes: number;
    totalPastes: number;
    activeUsers: number;
    totalUsers: number;

    weeklyPasteStats: {
        date: Date;
        created: number;
        deleted: number;
        expired: number;
        total: number;
        active: number;
    }[];
}

export const getVersion = async (fetchFunc: FetchFunc): Promise<string> => {
    const res = await fetchFunc(`${getApiUrl()}/meta/version`, {
        method: "GET"
    });

    if (res.ok) return (await res.json()).version;

    return "unknown version";
};

export const getReleases = async (fetchFunc: FetchFunc): Promise<Release[]> => {
    const res = await fetchFunc(`${getApiUrl()}/meta/releases`, {
        method: "GET"
    });

    if (res.ok) return await res.json();

    return [];
};

export const getActivePastes = async (fetchFunc: FetchFunc): Promise<number> => {
    const res = await fetchFunc(`${getApiUrl()}/meta/active_pastes`, {
        method: "GET"
    });

    if (res.ok) return (await res.json()).count;

    return 0;
};

export const getAppStats = async (fetchFunc: FetchFunc): Promise<[AppStats | null, number]> => {
    const res = await fetchFunc(`${getApiUrl()}/meta/stats`, {
        method: "GET"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};
