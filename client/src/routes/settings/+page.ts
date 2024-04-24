import { getSelf } from "$lib/api/auth";
import type { PageLoad } from "./$types";
import { redirect } from "@sveltejs/kit";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    if (self) {
        redirect(300, "/settings/profile");
    } else {
        redirect(300, "/settings/behaviour");
    }
};
