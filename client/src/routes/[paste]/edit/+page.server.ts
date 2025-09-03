import { getPaste, isPasteEncrypted } from "$lib/api/paste";
import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { getUserTags } from "$lib/api/user";

export const load: PageServerLoad = async ({ params, fetch, parent, request }) => {
    const { self } = await parent();

    if (!self) {
        error(401, "you must be logged in to edit a paste.");
    }

    const cookieHeader = request.headers.get("cookie") ?? "";

    const [paste, pasteStatus] = await getPaste(fetch, params.paste, cookieHeader);

    if (!paste) {
        const isEncrypted = await isPasteEncrypted(fetch, params.paste, cookieHeader);

        if (isEncrypted) {
            redirect(302, `/${params.paste}/decrypt`);
        } else {
            // TODO: error handling
            error(pasteStatus);
        }
    }

    if (paste.ownerId !== self.id) {
        error(403, "you do not have permission to edit this paste.");
    }

    const tags = await getUserTags(fetch, self.username, cookieHeader);

    return {
        paste: paste,
        userTags: tags
    };
};
