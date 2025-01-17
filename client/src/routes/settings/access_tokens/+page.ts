import { getAccessTokens } from "$lib/api/auth";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const accessTokens = await getAccessTokens(fetch);

    return { accessTokens };
};
