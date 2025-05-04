import { getAccessTokens } from "$lib/api/auth";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, parent }) => {
    const { self } = await parent();

    if (!self) throw error(401);

    const accessTokens = await getAccessTokens(fetch);

    return { accessTokens };
};
