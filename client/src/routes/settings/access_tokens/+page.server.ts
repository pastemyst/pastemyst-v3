import { getAccessTokens } from "$lib/api/auth";
import { error } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ fetch, parent, request }) => {
    const { self } = await parent();

    if (!self) throw error(401);

    const accessTokens = await getAccessTokens(fetch, request.headers.get("cookie") ?? "");

    return { accessTokens };
};
