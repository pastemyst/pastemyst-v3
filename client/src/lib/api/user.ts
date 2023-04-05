import { PUBLIC_API_BASE } from "$env/static/public";
import type { FetchFunc } from "./fetch";

export interface User {
    id: string;
    username: string;
    avatarId: string;
    createdAt: string;
    contributor: boolean;
    supporter: number;
}

export const getUserByUsername = async (username: string): Promise<User | null> => {
    const res = await fetch(`${PUBLIC_API_BASE}/users/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getUserById = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[User | null, number]> => {
    const res = await fetchFunc(`${PUBLIC_API_BASE}/users?id=${id}`, {
        method: "get"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};

export const getUserTags = async (fetchFunc: FetchFunc, username: string): Promise<string[]> => {
    const res = await fetchFunc(`${PUBLIC_API_BASE}/users/${username}/tags`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};
