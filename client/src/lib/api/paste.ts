import { apiBase } from "./api";
import type { FetchFunc } from "./fetch";
import type { LangStat } from "./lang";

export enum ExpiresIn {
    never = "never",
    oneHour = "1h",
    twoHours = "2h",
    tenHours = "10h",
    oneDay = "1d",
    twoDays = "2d",
    oneWeek = "1w",
    oneMonth = "1m",
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
    private: boolean;
    stars: number;
}

export interface Pasty {
    id: string;
    title: string;
    content: string;
    language: string;
}

export interface PasteStats {
    lines: number;
    words: number;
    size: number;
    pasties: { [id: string]: Stats };
}

export interface Stats {
    lines: number;
    words: number;
    size: number;
}

export interface PasteSkeleton {
    title: string;
    expiresIn: ExpiresIn;
    pasties: PastySkeleton[];
    anonymous: boolean;
    private: boolean;
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
        case "1 month":
            return ExpiresIn.oneMonth;
        case "1 year":
            return ExpiresIn.oneYear;
    }

    return ExpiresIn.never;
};

/**
 * Converts an `ExpiresIn` enum to a long string.
 */
export const expiresInToLongString = (exp: ExpiresIn): string => {
    switch (exp) {
        case ExpiresIn.never:
            return "never";
        case ExpiresIn.oneHour:
            return "1 hour";
        case ExpiresIn.twoHours:
            return "2 hours";
        case ExpiresIn.tenHours:
            return "10 hours";
        case ExpiresIn.oneDay:
            return "1 day";
        case ExpiresIn.twoDays:
            return "2 days";
        case ExpiresIn.oneWeek:
            return "1 week";
        case ExpiresIn.oneMonth:
            return "1 month";
        case ExpiresIn.oneYear:
            return "1 year";
    }
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

    return null;
};

export const getPaste = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[Paste | null, number]> => {
    const res = await fetchFunc(`${apiBase}/paste/${id}`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};

export const deletePaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${apiBase}/paste/${id}`, {
        method: "delete",
        credentials: "include"
    });

    return res.ok;
};

export const starPaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${apiBase}/paste/${id}/star`, {
        method: "post",
        credentials: "include"
    });

    return res.ok;
};

export const isPasteStarred = async (fetchFunc: FetchFunc, id: string): Promise<boolean> => {
    const res = await fetchFunc(`${apiBase}/paste/${id}/star`, {
        method: "get",
        credentials: "include"
    });

    return res.ok;
};

export const getPasteStats = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<PasteStats | null> => {
    const res = await fetchFunc(`${apiBase}/paste/${id}/stats`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getPasteLangs = async (fetchFunc: FetchFunc, id: string): Promise<LangStat[]> => {
    const res = await fetchFunc(`${apiBase}/paste/${id}/langs`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};
