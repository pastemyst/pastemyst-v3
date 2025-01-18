import {
    getPasteDiff,
    getPasteHistoryCompact,
    getPasteLangs,
    getPasteStats,
    isPasteEncrypted,
    isPasteStarred,
    type Pasty
} from "$lib/api/paste";
import { error, redirect } from "@sveltejs/kit";
import type { PageLoad } from "./$types";
import { getUserById } from "$lib/api/user";

export const load: PageLoad = async ({ params, fetch }) => {
    const pasteDiff = await getPasteDiff(fetch, params.paste, params.history);

    if (!pasteDiff) {
        const isEncrypted = await isPasteEncrypted(fetch, params.paste);

        if (isEncrypted) {
            redirect(302, `/${params.paste}/decrypt`);
        } else {
            // TODO: error handling
            error(404);
        }
    }

    const paste = pasteDiff.currentPaste;

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

    const currentHistoryIndex = history.findIndex((h) => h.id === params.history);

    type HistoryType = (typeof history)[0];

    const previousEdit: HistoryType | undefined = history[currentHistoryIndex + 1];
    const nextEdit: HistoryType | undefined = history[currentHistoryIndex - 1];

    const oldTitle = pasteDiff.oldPaste.title;
    const newTitle = pasteDiff.newPaste.title;

    const addedPasties = pasteDiff.newPaste.pasties.filter(
        (p) => !pasteDiff.oldPaste.pasties.some((p2) => p2.id === p.id)
    );
    const deletedPasties = pasteDiff.oldPaste.pasties.filter(
        (p) => !pasteDiff.newPaste.pasties.some((p2) => p2.id === p.id)
    );

    const modifiedPasties: { oldPasty: Pasty; newPasty: Pasty }[] = [];
    for (const newPasty of pasteDiff.newPaste.pasties) {
        const oldPasty = pasteDiff.oldPaste.pasties.find((p2) => p2.id === newPasty.id);
        if (oldPasty && oldPasty.content !== newPasty.content) {
            modifiedPasties.push({ oldPasty, newPasty });
        }
    }

    return {
        paste,
        owner,
        pasteStats,
        langStats,
        isStarred,
        previousEdit,
        nextEdit,
        historyId: params.history,
        oldTitle,
        newTitle,
        addedPasties,
        deletedPasties,
        modifiedPasties
    };
};
