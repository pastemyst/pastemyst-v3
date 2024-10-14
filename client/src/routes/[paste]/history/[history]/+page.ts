import { getPasteAtEdit, getPasteHistoryCompact, getPasteLangs, getPasteStats, isPasteStarred } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";
import { getUserById } from "$lib/api/user";

export const load: PageLoad = async ({ params, fetch, parent }) => {
    const paste = await getPasteAtEdit(fetch, params.paste, params.history);

    if (!paste) {
	error(404);
    }

    const history = await getPasteHistoryCompact(fetch, params.paste);

    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

    const pasteStats = await getPasteStats(fetch, paste.id);
    const langStats = await getPasteLangs(fetch, paste.id);
    const isStarred = await isPasteStarred(fetch, paste.id);

    const { settings } = await parent();

    const highlightedCode: string[] = [];

    for (const pasty of paste.pasties) {
        const res = await fetch("/internal/highlight", {
            method: "post",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                content: pasty.content,
                language: pasty.language,
                wrap: settings.textWrap,
                theme: settings.theme
            })
        });

        // TODO: error handling
        if (!res.ok) error(500);

        highlightedCode.push(await res.text());
    }

    const currentHistoryIndex = history.findIndex(h => h.id === params.history);

    type HistoryType = typeof history[0];

    const previousEdit: HistoryType | undefined = history[currentHistoryIndex + 1];
    const nextEdit: HistoryType | undefined = history[currentHistoryIndex - 1];

    return {
	paste, owner, pasteStats, langStats, isStarred, highlightedCode, previousEdit, nextEdit
    };
};
