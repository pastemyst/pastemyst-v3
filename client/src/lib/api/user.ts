import { API_URL, type FetchFunc } from "./fetch";

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
    const res = await fetch(`${API_URL}/users/${username}`, {
        method: "GET"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getUserById = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[User | null, number]> => {
    const res = await fetchFunc(`${API_URL}/users?id=${id}`, {
        method: "GET"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};

export const getUserTags = async (fetchFunc: FetchFunc, username: string): Promise<string[]> => {
    const res = await fetchFunc(`${API_URL}/users/${username}/tags`, {
        method: "GET",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};

export const deleteUser = async (username: string): Promise<boolean> => {
    const res = await fetch(`${API_URL}/users/${username}`, {
        method: "DELETE",
        credentials: "include"
    });

    return res.ok;
};
