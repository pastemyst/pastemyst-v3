import { fetcherPost } from "./fetcher";

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

export const createPaste = async(skeleton: PasteSkeleton): Promise<Paste> => {
    const res = await fetcherPost<Paste>("/api/v3/paste/", {
        body: JSON.stringify(skeleton)
    });

    if (res.ok) return res.data;

    // TODO: error handling
    return null;
};
