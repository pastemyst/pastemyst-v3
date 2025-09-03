import { getPaste, getPasteAtEdit, type Paste } from "$lib/api/paste";
import { error, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async ({ params, fetch, request }) => {
    if (!params.paste) {
        error(404);
    }

    const cookieHeader = request.headers.get("cookie") ?? "";

    let paste: Paste | null = null;
    if (params.history) {
        paste = await getPasteAtEdit(fetch, params.paste, params.history, cookieHeader);
    } else {
        [paste] = await getPaste(fetch, params.paste, cookieHeader);
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
