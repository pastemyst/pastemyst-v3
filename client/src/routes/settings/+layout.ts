import { getSelf } from "$lib/api/auth";
import type { LayoutLoad } from "./$types";

export const load: LayoutLoad = async ({ fetch, url }) => {
    const self = await getSelf(fetch);

    const category = url.pathname.substring(url.pathname.lastIndexOf("/") + 1);

    return {
        self: self,
        category: category
    };
};
