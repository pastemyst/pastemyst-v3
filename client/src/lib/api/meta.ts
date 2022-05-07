import { apiBase } from "./api";
import { fetcherGet } from "./fetcher";

export const getVersion = async (): Promise<string> => {
    interface VersionRes {
        version: string;
    }

    const res = await fetcherGet<VersionRes>(`${apiBase}/meta/version`);

    if (res.ok) return res.data.version;

    return "unknown version";
};

export const getActivePastes = async (): Promise<number> => {
    interface ActivePastesRes {
        count: number;
    }

    const res = await fetcherGet<ActivePastesRes>(`${apiBase}/meta/active_pastes`);

    if (res.ok) return res.data.count;

    return 0;
};