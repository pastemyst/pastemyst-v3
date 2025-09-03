import { browser } from "$app/environment";
import { env } from "$env/dynamic/public";

export type FetchFunc = (info: RequestInfo, init?: RequestInit | undefined) => Promise<Response>;

export const getApiUrl = () => {
    return browser ? env.PUBLIC_API_CLIENT_BASE : env.PUBLIC_API_SERVER_BASE;
};
