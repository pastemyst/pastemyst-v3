import { browser } from "$app/environment";
import { env } from "$env/dynamic/public";
import { getSelf } from "$lib/api/auth";
import { getUserTags } from "$lib/api/user";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username);

        // if logged in and there's no settings saved in the localstorage
        // load the profile settings and save it to localstorage
        if (browser) {
            if (!localStorage.getItem("settings")) {
                const settingsRequest = await fetch(`${env.PUBLIC_API_BASE}/settings`, {
                    method: "get",
                    credentials: "include"
                });

                if (settingsRequest.ok) {
                    const settings = await settingsRequest.json();
                    localStorage.setItem("settings", JSON.stringify(settings));
                }
            }
        }
    }

    return {
        userTags: tags
    };
};
