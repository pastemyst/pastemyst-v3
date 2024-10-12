import { getPaste, getPasteHistoryCompact } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const [paste, pasteStatus] = await getPaste(fetch, params.paste);

    if (!paste) {
        // TODO: error handling
        error(pasteStatus);
    }

    const history = await getPasteHistoryCompact(fetch, params.paste);

    if (history.length === 0) {
        error(404);
    }

    return {
        history, paste
    };
};
