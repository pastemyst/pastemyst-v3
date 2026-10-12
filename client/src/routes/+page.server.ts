import { getUserTags } from "$lib/api/user";
import { getPaste, isPasteEncrypted, type Paste } from "$lib/api/paste";
import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ parent, request, url, fetch }) => {
    const { self } = await parent();

    const cookie = request.headers.get("cookie") ?? "";

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username, cookie);
    }

    // ?clone={id} fills the editor with a copy of that paste
    let clonedPaste: Paste | null = null;
    const cloneId = url.searchParams.get("clone");
    if (cloneId) {
        let status: number;
        [clonedPaste, status] = await getPaste(fetch, cloneId, cookie);

        if (!clonedPaste) {
            // An encrypted paste can be cloned once it's decrypted (the key is kept in a cookie)
            if (await isPasteEncrypted(fetch, cloneId, cookie)) {
                redirect(302, `/${cloneId}/decrypt`);
            }

            error(status);
        }
    }

    return {
        userTags: tags,
        clonedPaste: clonedPaste
    };
};
