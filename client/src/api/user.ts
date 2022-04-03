export interface User {
    id: string;
    username: string;
    avatarUrl: string;
}

export const getUser = async (username: string): Promise<User> => {
    const res = await fetch(`/api/v3/user/${username}`);

    if (!res.ok) return null;

    return await res.json();
};