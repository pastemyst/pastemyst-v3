import { apiBase } from "./api";

export interface User {
    id: string;
    username: string;
    avatarUrl: string;
    createdAt: string;
    contributor: boolean;
    supporter: number;
}

export const getUserByUsername = async (username: string): Promise<User | null> => {
    const res = await fetch(`${apiBase}/user/by_username/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getUserById = async (id: string): Promise<User | null> => {
    const res = await fetch(`${apiBase}/user/by_id/${id}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};
