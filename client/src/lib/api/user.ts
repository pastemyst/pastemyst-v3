import { apiBase } from "./api";

export interface User {
    id: string;
    username: string;
    avatarUrl: string;
    createdAt: string;
}

export const getUser = async (username: string): Promise<User> => {
    const res = await fetch(`${apiBase}/user/${username}`, {
        method: "get"
    });

    if (res.ok) return await res.json();

    return null;
};
