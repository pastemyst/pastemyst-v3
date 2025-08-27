import type { PageLoad } from "./$types";
import { redirect } from "@sveltejs/kit";

export const load: PageLoad = async ({ parent }) => {
    const { self } = await parent();

    if (self) {
        redirect(300, "/settings/profile");
    } else {
        redirect(300, "/settings/behaviour");
    }
};
