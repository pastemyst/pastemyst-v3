import { error } from "@sveltejs/kit";
import { ExpiresIn, getPaste, getPasteLangs, getPasteStats } from "$lib/api/paste";
import type { PageLoad } from "./$types";
import moment from "moment";
import { getUserById } from "$lib/api/user";

export const load: PageLoad = async ({ params, fetch }) => {
    const [paste, pasteStatus] = await getPaste(fetch, params.paste);

    if (!paste) {
        // TODO: error handling
        throw error(pasteStatus);
    }

    const relativeCreatedAt = moment(paste.createdAt).fromNow();
    const relativesExpiresIn =
        paste.expiresIn !== ExpiresIn.never ? moment(paste.deletesAt).fromNow() : "";
    const pasteStats = await getPasteStats(fetch, paste.id);
    const langStats = await getPasteLangs(fetch, paste.id);
    const [owner, ownerStatus] =
        paste.ownerId !== "" ? await getUserById(fetch, paste.ownerId) : [null, 0];

    if (paste.ownerId !== "" && !owner) {
        // TODO: error handling
        throw error(ownerStatus);
    }

    const highlightedCode: string[] = [];

    for (const pasty of paste.pasties) {
        const res = await fetch("/internal/highlight", {
            method: "post",
            body: JSON.stringify({
                content: pasty.content,
                language: pasty.language
            })
        });

        // TODO: error handling
        if (!res.ok) throw error(500);

        highlightedCode.push(await res.text());
    }

    return {
        paste: paste,
        relativeCreatedAt: relativeCreatedAt,
        relativesExpiresIn: relativesExpiresIn,
        langStats: langStats,
        pasteStats: pasteStats,
        owner: owner,
        highlightedCode: highlightedCode
    };
};
