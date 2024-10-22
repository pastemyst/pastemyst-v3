import { env } from "$env/dynamic/public";
import type { IndentUnit } from "$lib/indentation";
import type { FetchFunc } from "./fetch";

export interface UserSettings {
    showAllPastesOnProfile: boolean;
}

export interface Settings {
    defaultLanguage: string;
    defaultIndentationUnit: IndentUnit;
    defaultIndentationWidth: number;
    textWrap: boolean;
    copyLinkOnCreate: boolean;
    pasteView: "tabbed" | "stacked";
    theme: string;
}

export const getSettings = async (
    fetchFunc: FetchFunc
): Promise<[settings: Settings, cookie?: string]> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/settings`, {
        method: "GET",
        credentials: "include"
    });

    const cookies = res.headers.getSetCookie();

    // TODO: handle failure
    return [await res.json(), cookies[0]];
};

export const updateSettings = async (fetchFunc: FetchFunc, settings: Settings) => {
    await fetchFunc(`${env.PUBLIC_API_BASE}/settings`, {
        method: "PATCH",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(settings)
    });
};

export const getUserSettings = async (fetchFunc: FetchFunc): Promise<UserSettings> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/settings/user`, {
        method: "GET",
        credentials: "include"
    });

    return await res.json();
};

export const updateUserSettings = async (fetchFunc: FetchFunc, userSettings: UserSettings) => {
    await fetchFunc(`${env.PUBLIC_API_BASE}/settings/user`, {
        method: "PATCH",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(userSettings)
    });
};
