import { getUserSettings } from "$lib/api/settings";
import { error } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ fetch, parent, request }) => {
    const { self } = await parent();

    if (!self) {
        error(401, "You must be logged in to access settings.");
    }

    const userSettings = await getUserSettings(fetch, request.headers.get("cookie") ?? "");

    return {
        self: self,
        userSettings: userSettings
    };
};
