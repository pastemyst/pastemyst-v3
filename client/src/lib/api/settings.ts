import { env } from "$env/dynamic/public";
import type { FetchFunc } from "./fetch";

export type SettingsContext = "browser" | "profile";

export interface UserSettings {
    showAllPastesOnProfile: boolean;
}

export const getUserSettings = async (fetchFunc: FetchFunc): Promise<UserSettings> => {
    const res = await fetchFunc(`${env.PUBLIC_API_BASE}/settings`, {
        method: "get",
        credentials: "include"
    });

    return await res.json();
};

export const updateUserSettings = async (fetchFunc: FetchFunc, userSettings: UserSettings) => {
    await fetchFunc(`${env.PUBLIC_API_BASE}/settings`, {
        method: "patch",
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(userSettings)
    });
};
