import { apiBase } from "./api";
import { fetcherGet } from "./fetcher";

export interface Release {
    url: string;
    title: string;
    content: string;
    isPrerelease: boolean;
    releasedAt: string;
}

export const getVersion = async (): Promise<string> => {
    interface VersionRes {
        version: string;
    }

    const res = await fetcherGet<VersionRes>(`${apiBase}/meta/version`);

    if (res.ok) return res.data.version;

    return "unknown version";
};

export const getReleases = async(): Promise<Release[]> => {
    const res = await fetcherGet<Release[]>(`${apiBase}/meta/releases`);

    if (res.ok) return res.data;

    return [];
};

export const getActivePastes = async (): Promise<number> => {
    interface ActivePastesRes {
        count: number;
    }

    const res = await fetcherGet<ActivePastesRes>(`${apiBase}/meta/active_pastes`);

    if (res.ok) return res.data.count;

    return 0;
};