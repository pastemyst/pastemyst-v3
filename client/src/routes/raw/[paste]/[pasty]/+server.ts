import { getPaste } from "$lib/api/paste";
import { error, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async ({ params }) => {
    if (!params.paste) {
        throw error(404);
    }

    const [paste, pasteRes] = await getPaste(fetch, params.paste);

    if (!paste) {
        throw error(pasteRes);
    }

    const pasty = paste.pasties.find((p) => p.id === params.pasty);

    if (!pasty) {
        throw error(404);
    }

    return new Response(pasty.content);
};
