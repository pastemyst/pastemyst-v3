import {
    getPaste,
    getPasteHistoryCompact,
    getPasteLangs,
    getPasteStats,
    isPasteStarred
} from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";
import { getUserById } from "$lib/api/user";

export const load: PageLoad = async ({ params, fetch }) => {
    const [paste, pasteStatus] = await getPaste(fetch, params.paste);

    if (!paste) {
        // TODO: error handling
        error(pasteStatus);
    }

    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

    const pasteStats = await getPasteStats(fetch, paste.id);
    const langStats = await getPasteLangs(fetch, paste.id);
    const isStarred = await isPasteStarred(fetch, paste.id);

    const history = await getPasteHistoryCompact(fetch, params.paste);

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
