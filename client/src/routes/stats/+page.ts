import { getAppStats } from "$lib/api/meta";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const [stats, statsStatus] = await getAppStats(fetch);

    if (stats === null) error(statsStatus);

    return stats;
};
