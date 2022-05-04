import { apiBase } from "./api";
import { fetcherGet } from "./fetcher";

export interface User {
    _id: string;
    username: string;
    avatarUrl: string;
}

export const getUser = async (username: string): Promise<User> => {
    const res = await fetcherGet<User>(`${apiBase}/user/${username}`);

    if (!res.ok) return null;

    return res.data;
};
