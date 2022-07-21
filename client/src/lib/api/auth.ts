import { apiBase } from "./api";
import type { User } from "./user";

export const createAccount = async (username: string): Promise<string | null> => {
    const data = {
        username: username
    };

    const res = await fetch(`${apiBase}/auth/register`, {
        method: "post",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) return (await res.json()).message;

    return null;
};

export const getSelf = async (): Promise<User | null> => {
    const res = await fetch(`${apiBase}/auth/self`, {
        method: "get",
        credentials: "include"
    })

    if (res.ok) return await res.json();

    return null;
};
