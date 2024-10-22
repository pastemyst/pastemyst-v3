import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";
import type { User } from "./user";

export const createAccount = async (username: string): Promise<string | null> => {
    const data = {
        username: username
    };

    const res = await fetch(`${env.PUBLIC_API_BASE}/auth/register`, {
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

export const getSelf = async (fetchFunc: FetchFunc): Promise<User | null> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/auth/self`, {
        method: "GET",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return null;
};
