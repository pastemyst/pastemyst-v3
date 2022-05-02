import { fetcherGet } from "./fetcher";

export interface User {
    id: string;
    username: string;
    avatarUrl: string;
}

export const getUser = async (username: string): Promise<User> => {
    const res = await fetcherGet<User>(`/api/v3/user/${username}`);

    if (!res.ok) return null;

    return res.data;
};
