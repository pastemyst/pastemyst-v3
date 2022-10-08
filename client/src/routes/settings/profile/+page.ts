import { getSelf } from "$lib/api/auth";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    if (!self) {
        throw error(401, "You must be logged in to access settings.");
    }

    return {
        self: self
    }
};
