import { apiBase } from "./api";

export enum ExpiresIn {
    never = "never",
    oneHour = "1h",
    twoHours = "2h",
    tenHours = "10h",
    oneDay = "1d",
    twoDays = "2d",
    oneWeek = "1w",
    oneYear = "1y"
}

export interface Paste {
    id: string;
    title: string;
    createdAt: string;
    expiresIn: ExpiresIn;
    deletesAt: string;
    pasties: Pasty[];
    ownerId: string;
}

export interface Pasty {
    id: string;
    title: string;
    content: string;
    language: string;
}

export interface PasteSkeleton {
    title: string;
    expiresIn: ExpiresIn;
    pasties: PastySkeleton[];
    anonymous: boolean;
}

export interface PastySkeleton {
    title: string;
    content: string;
    language: string;
}

/**
 * Converts a long string to `ExpiresIn` enum.
 */
export const expiresInFromString = (s: string): ExpiresIn => {
    switch (s) {
        case "never":
            return ExpiresIn.never;
        case "1 hour":
            return ExpiresIn.oneHour;
        case "2 hours":
            return ExpiresIn.twoHours;
        case "10 hours":
            return ExpiresIn.tenHours;
        case "1 day":
            return ExpiresIn.oneDay;
        case "2 days":
            return ExpiresIn.twoDays;
        case "1 week":
            return ExpiresIn.oneWeek;
        case "1 year":
            return ExpiresIn.oneYear;
    }

    return ExpiresIn.never;
};

export const createPaste = async (skeleton: PasteSkeleton): Promise<Paste | null> => {
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
