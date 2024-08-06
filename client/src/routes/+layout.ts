import { getSelf } from "$lib/api/auth";
import { getActivePastes, getVersion } from "$lib/api/meta";
import type { LayoutLoad } from "./$types";

export const load: LayoutLoad = async ({ fetch, data }) => {
    const self = await getSelf(fetch);
    const version = await getVersion(fetch);
    const activePastes = await getActivePastes(fetch);

    return {
        self: self,
        version: version,
        activePastes: activePastes,
        settings: data.settings
    };
};
