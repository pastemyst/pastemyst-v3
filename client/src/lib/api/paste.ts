import { apiBase } from "./api";

export interface Paste {
    id: string;
    title: string;
    createdAt: Date;
    pasties: Pasty[];
}

export interface Pasty {
    id: string;
    title: string;
    content: string;
}

export interface PasteSkeleton {
    title: string;
    pasties: PastySkeleton[];
}

export interface PastySkeleton {
    title: string;
    content: string;
}

export const createPaste = async (skeleton: PasteSkeleton): Promise<Paste> => {
    const res = await fetch(`${apiBase}/paste/`, {
        method: "post",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(skeleton)
    });

    if (res.ok) return await res.json();

    // TODO: error handling
    return null;
};
