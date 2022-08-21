import { getReleases } from "$lib/api/meta";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const releases = await getReleases(fetch);

    return {
        releases: releases
    };
};
