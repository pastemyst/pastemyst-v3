import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";
import type { User } from "./user";

export const getSelf = async (fetchFunc: FetchFunc): Promise<User | null> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/auth/self`, {
        method: "GET",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return null;
};
