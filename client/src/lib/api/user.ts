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
    const res = await fetch(`${PUBLIC_API_BASE}/user/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};

export const doesUserExist = async (username: string): Promise<boolean> => {
    const res = await fetch(`${PUBLIC_API_BASE}/user/exists?username=${username}`, {
        method: "get"
    });

    return (await res.json()).exists;
};

export const getUserById = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[User | null, number]> => {
    const res = await fetchFunc(`${PUBLIC_API_BASE}/user?id=${id}`, {
        method: "get"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};
