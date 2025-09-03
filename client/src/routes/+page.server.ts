import { getUserTags } from "$lib/api/user";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ parent, request }) => {
    const { self } = await parent();

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username, request.headers.get("cookie") ?? "");
    }

    return {
        userTags: tags
    };
};
