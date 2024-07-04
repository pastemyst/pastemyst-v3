import { env } from "$env/dynamic/public";
import type { IndentUnit } from "$lib/indentation";
import { settingsStore } from "$lib/stores";
import type { FetchFunc } from "./fetch";

export type SettingsContext = "browser" | "profile";

export interface UserSettings {
    showAllPastesOnProfile: boolean;
}

export interface Settings {
    defaultLanguage: string;
    defaultIndentationUnit: IndentUnit;
    defaultIndentationWidth: number;
    textWrap: boolean;
}

export const defaultSettings: Settings = {
    defaultLanguage: "Text",
    defaultIndentationUnit: "spaces",
    defaultIndentationWidth: 4,
    textWrap: true
};

export const getLocalSettings = (): Settings => {
    const settings = localStorage.getItem("settings");

    if (!settings) return defaultSettings;

    const settingsObj = JSON.parse(settings) as Settings;

    // if there are any undefined settings set them to default
    // this can happen if new settings are added over time
    // but the localstorage settings doesn't contain the new settings
    let setting: keyof typeof settingsObj;
    for (setting in defaultSettings) {
        if (settingsObj[setting] === null || settingsObj === undefined) {
            settingsObj[setting] = defaultSettings[setting] as never;
        }
    }

    return settingsObj;
};

export const updateSettings = async (
    fetchFunc: FetchFunc,
    context: SettingsContext,
    settings: Settings
) => {
    if (context === "browser") {
        localStorage.setItem("settings", JSON.stringify(settings));

        settingsStore.set(settings);
    } else {
        const res = await fetch(`${env.PUBLIC_API_BASE}/settings`, {
            method: "get",
            credentials: "include"
        });

        // if the user changed their profile settings
        // and the browser settings are unchanged
        // also set the browser settings
        const previousSettings = await res.json();
        const localSettings = getLocalSettings();

        if (JSON.stringify(previousSettings) === JSON.stringify(localSettings)) {
            updateSettings(fetchFunc, "browser", settings);
        }

        await fetchFunc(`${env.PUBLIC_API_BASE}/settings`, {
            method: "patch",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(settings)
        });
    }
};

export const getUserSettings = async (fetchFunc: FetchFunc): Promise<UserSettings> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/settings/user`, {
        method: "get",
        credentials: "include"
    });

    return await res.json();
};

export const updateUserSettings = async (fetchFunc: FetchFunc, userSettings: UserSettings) => {
    await fetchFunc(`${env.PUBLIC_API_BASE}/settings/user`, {
        method: "patch",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(userSettings)
    });
};
