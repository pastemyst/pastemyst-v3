import { getApiUrl, type FetchFunc } from "./fetch";
import type { ExpiresIn } from "./paste";
import type { User } from "./user";

export interface AccessToken {
    id: string;
    description: string;
    createdAt: string;
    expiresAt?: string;
    scopes: string[];
}

export interface GenerateAccessTokenInfo {
    scopes: string[];
    description?: string;
    expiresIn: ExpiresIn;
}

export interface GenerateAccessTokenResponse {
    accessToken: string;
    expiresAt?: string;
}

export const createAccount = async (username: string): Promise<string | null> => {
    const data = {
        username: username
    };

    const res = await fetch(`${getApiUrl()}/auth/register`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) return (await res.json()).message;

    return null;
};

export const getSelf = async (fetchFunc: FetchFunc, cookie?: string): Promise<User | null> => {
    const res = await fetchFunc(`${getApiUrl()}/auth/self`, {
        method: "GET",
        credentials: "include",
        headers: {
            cookie: cookie ?? ""
        }
    });

    if (res.ok) return await res.json();

    return null;
};

export const getAccessTokens = async (
    fetchFunc: FetchFunc,
    cookie?: string
): Promise<AccessToken[]> => {
    const res = await fetchFunc(`${getApiUrl()}/auth/self/access_tokens`, {
        method: "GET",
        credentials: "include",
        headers: {
            cookie: cookie ?? ""
        }
    });

    if (res.ok) return await res.json();

    return [];
};

export const generateAccessToken = async (
    fetchFunc: FetchFunc,
    generateInfo: GenerateAccessTokenInfo
): Promise<GenerateAccessTokenResponse | undefined> => {
    const res = await fetchFunc(`${getApiUrl()}/auth/self/access_tokens`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(generateInfo)
    });

    if (res.ok) return await res.json();

    return undefined;
};

export const deleteAccessToken = async (fetchFunc: FetchFunc, id: string) => {
    await fetchFunc(`${getApiUrl()}/auth/self/access_tokens/${id}`, {
        method: "DELETE",
        credentials: "include"
    });
};
