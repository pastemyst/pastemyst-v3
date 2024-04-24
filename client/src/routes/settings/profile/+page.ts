import { getSelf } from "$lib/api/auth";
import { getUserSettings } from "$lib/api/settings";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    if (!self) {
        error(401, "You must be logged in to access settings.");
    }

    const userSettings = await getUserSettings(fetch);

    return {
        self: self,
        userSettings: userSettings
    };
};
