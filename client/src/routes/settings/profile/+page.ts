import { getSelf } from "$lib/api/auth";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    return {
        self: self
    }
};
