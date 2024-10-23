import { getPaste, getPasteAtEdit, type Paste } from "$lib/api/paste";
import { error, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async ({ params }) => {
    if (!params.paste) {
        error(404);
    }

    let paste: Paste | null = null;
    if (params.history) {
        paste = await getPasteAtEdit(fetch, params.paste, params.history);
    } else {
        [paste] = await getPaste(fetch, params.paste);
    }

    if (!paste) {
        error(404);
    }

    const pasty = paste.pasties.find((p) => p.id === params.pasty);

    if (!pasty) {
        error(404);
    }

    return new Response(pasty.content);
};
