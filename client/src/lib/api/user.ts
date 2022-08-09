import { apiBase } from "./api";
import type { FetchFunc } from "./fetch";

export interface User {
    id: string;
    username: string;
    avatarUrl: string;
    createdAt: string;
    contributor: boolean;
    supporter: number;
}

export const getUserByUsername = async (username: string): Promise<User | null> => {
    const res = await fetch(`${apiBase}/user/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getUserById = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[User | null, number]> => {
    const res = await fetchFunc(`${apiBase}/user?id=${id}`, {
        method: "get"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};
