import { getApiUrl, type FetchFunc } from "./fetch";

export interface Announcement {
    id: string;
    title: string;
    content: string;
    createdAt: string;
}

export const getLatestAnnouncement = async (
    fetch: FetchFunc
): Promise<Announcement | undefined> => {
    const res = await fetch(`${getApiUrl()}/announcements/latest`, {
        method: "GET",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return undefined;
};

export const getAllAnnouncements = async (
    fetch: FetchFunc,
    cookie?: string
): Promise<Announcement[]> => {
    const res = await fetch(`${getApiUrl()}/announcements`, {
        method: "GET",
        credentials: "include",
        headers: {
            cookie: cookie ?? ""
        }
    });

    return await res.json();
};

export const createAnnouncement = async (fetch: FetchFunc, title: string, content: string) => {
    await fetch(`${getApiUrl()}/announcements`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ title, content })
    });
};

export const deleteAnnouncement = async (fetch: FetchFunc, id: string) => {
    await fetch(`${getApiUrl()}/announcements/${id}`, {
        method: "DELETE",
        credentials: "include"
    });
};

export const editAnnouncement = async (
    fetch: FetchFunc,
    id: string,
    title: string,
    content: string
) => {
    await fetch(`${getApiUrl()}/announcements/${id}`, {
        method: "PATCH",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ title, content })
    });
};
