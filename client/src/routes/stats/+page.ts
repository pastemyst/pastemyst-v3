import { getAppStats } from "$lib/api/meta";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const [stats, statsStatus] = await getAppStats(fetch);

    if (stats === null) throw(statsStatus);

    return stats;
};
