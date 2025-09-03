import type { PageServerLoad } from "./$types";
import { redirect } from "@sveltejs/kit";

export const load: PageServerLoad = async ({ parent }) => {
    const { self } = await parent();

    if (self) {
        redirect(302, "/settings/profile");
    } else {
        redirect(302, "/settings/behaviour");
    }
};
