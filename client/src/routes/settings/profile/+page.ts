import { getUserSettings } from "$lib/api/settings";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, parent }) => {
    const { self } = await parent();

    if (!self) {
        error(401, "You must be logged in to access settings.");
    }

    const userSettings = await getUserSettings(fetch);

    return {
        self: self,
        userSettings: userSettings
    };
};
