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