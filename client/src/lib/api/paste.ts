import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";
import type { LangStat } from "./lang";
import type { Page } from "./page";

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
    editedAt?: string;
    expiresIn: ExpiresIn;
    deletesAt: string;
    pasties: Pasty[];
    ownerId: string;
    private: boolean;
    pinned: boolean;
    stars: number;
    tags: string[];
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
    bytes: number;
    pasties: { [id: string]: Stats };
}

export interface Stats {
    lines: number;
    words: number;
    bytes: number;
}

export interface PasteCreateInfo {
    title: string;
    expiresIn: ExpiresIn;
    pasties: PastyCreateInfo[];
    anonymous: boolean;
    private: boolean;
    pinned: boolean;
    tags: string[];
}

export interface PastyCreateInfo {
    title: string;
    content: string;
    language: string;
}

export interface PasteEditInfo {
    title: string;
    tags: string[];
    pasties: PastyEditInfo[];
}

export interface PastyEditInfo {
    id?: string;
    title: string;
    content: string;
    language: string;
}

export interface PasteWithLangStats {
    paste: Paste;
    languageStats: LangStat[];
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

export const createPaste = async (createInfo: PasteCreateInfo): Promise<Paste | null> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/`, {
        method: "post",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(createInfo)
    });

    if (res.ok) return await res.json();

    return null;
};

export const editPaste = async (id: string, editInfo: PasteEditInfo): Promise<Paste | null> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/${id}`, {
        method: "patch",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(editInfo)
    });

    if (res.ok) return await res.json();

    return null;
};

export const getPaste = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<[Paste | null, number]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return [await res.json(), res.status];

    return [null, res.status];
};

export const getPasteHistoryCompact = async (fetchFunc: FetchFunc, id: string): Promise<{ id: string, editedAt: string }[]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}/history_compact`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};

export const getPasteAtEdit = async (fetchFunc: FetchFunc, id: string, historyId: string): Promise<Paste | null> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}/history/${historyId}`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return null;
};

export const deletePaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/${id}`, {
        method: "delete",
        credentials: "include"
    });

    return res.ok;
};

export const starPaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/${id}/star`, {
        method: "post",
        credentials: "include"
    });

    return res.ok;
};

export const pinPaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/${id}/pin`, {
        method: "post",
        credentials: "include"
    });

    return res.ok;
};

export const isPasteStarred = async (fetchFunc: FetchFunc, id: string): Promise<boolean> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}/star`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return false;
};

export const togglePrivatePaste = async (id: string): Promise<boolean> => {
    const res = await fetch(`${env.PUBLIC_API_BASE}/pastes/${id}/private`, {
        method: "post",
        credentials: "include"
    });

    return res.ok;
};

export const getPasteStats = async (
    fetchFunc: FetchFunc,
    id: string
): Promise<PasteStats | null> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}/stats`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return null;
};

export const getPasteLangs = async (fetchFunc: FetchFunc, id: string): Promise<LangStat[]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/pastes/${id}/langs`, {
        method: "get",
        credentials: "include"
    });

    if (res.ok) return await res.json();

    return [];
};

export const getUserPastes = async (
    fetchFunc: FetchFunc,
    username: string,
    pinned: boolean,
    page: number,
    pageSize: number
): Promise<Page<PasteWithLangStats> | null> => {
    const res = await fetchFunc(
        `${env.PUBLIC_API_BASE}/users/${username}/pastes${pinned ? "/pinned" : ""}` +
            `?page=${page}` +
            `&pageSize=${pageSize}`,
        {
            method: "get",
            credentials: "include"
        }
    );

    if (res.ok) return await res.json();

    return null;
};
