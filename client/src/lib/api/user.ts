import { apiBase } from "./api";

export interface User {
    id: string;
    username: string;
    avatarUrl: string;
    createdAt: string;
    contributor: boolean;
    supporter: number;
}

export const getUser = async (username: string): Promise<User | null> => {
    const res = await fetch(`${apiBase}/user/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};
