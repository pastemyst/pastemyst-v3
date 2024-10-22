import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";

export interface User {
    id: string;
    username: string;
    avatarId: string;
    createdAt: string;
    isContributor: boolean;
    isSupporter: boolean;
    isAdmin: boolean;
}

export const getUserByUsername = async (username: string): Promise<User | null> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/users/${username}`, {
        method: "GET"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getUserById = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[User | null, number]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/users?id=${id}`, {
        method: "GET"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};

export const getUserTags = async (fetchFunc: FetchFunc, username: string): Promise<string[]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/users/${username}/tags`, {
        method: "GET",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};

export const deleteUser = async (username: string): Promise<boolean> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/users/${username}`, {
        method: "DELETE",
        credentials: "include"
    });

    return res.ok;
};
