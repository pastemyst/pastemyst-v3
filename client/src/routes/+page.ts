import { getUserTags } from "$lib/api/user";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ parent }) => {
    const { self } = await parent();

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username);
    }

    return {
        userTags: tags
    };
};
