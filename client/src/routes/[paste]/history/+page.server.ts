import {
    getPaste,
    getPasteHistoryCompact,
    getPasteLangs,
    getPasteStats,
    isPasteEncrypted,
    isPasteStarred
} from "$lib/api/paste";
import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { getUserById } from "$lib/api/user";

export const load: PageServerLoad = async ({ params, fetch, request }) => {
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

    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

    const pasteStats = await getPasteStats(fetch, paste.id, cookieHeader);
    const langStats = await getPasteLangs(fetch, paste.id, cookieHeader);
    const isStarred = await isPasteStarred(fetch, paste.id, cookieHeader);

    const history = await getPasteHistoryCompact(fetch, params.paste, cookieHeader);

    if (history.length === 0) {
        error(404);
    }

    return {
        history,
        paste,
        owner,
        pasteStats,
        langStats,
        isStarred
    };
};
