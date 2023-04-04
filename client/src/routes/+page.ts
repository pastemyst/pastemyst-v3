import { getSelf } from "$lib/api/auth";
import { getUserTags } from "$lib/api/user";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const self = await getSelf(fetch);

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username);
    }

    return {
        userTags: tags
    }
};
