import { env } from "$env/dynamic/public";
import { getSelf } from "$lib/api/auth";
import type { Settings } from "$lib/api/settings";
import type { LayoutLoad } from "./$types";

export const load: LayoutLoad = async ({ fetch, url }) => {
    const self = await getSelf(fetch);

    const category = url.pathname.substring(url.pathname.lastIndexOf("/") + 1);

    let settings: Settings | null = null;
    if (self) {
        const res = await fetch(`${env.PUBLIC_API_BASE}/settings`, {
            method: "get",
            credentials: "include"
        });

        settings = await res.json();
    }

    return {
        self: self,
        category: category,
        settings: settings
    };
};
