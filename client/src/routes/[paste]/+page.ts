import { error } from "@sveltejs/kit";
import { ExpiresIn, getPaste, getPasteLangs, getPasteStats, isPasteStarred } from "$lib/api/paste";
import type { PageLoad } from "./$types";
import { getUserById } from "$lib/api/user";
import { formatDistanceToNow } from "date-fns";

export const load: PageLoad = async ({ params, fetch, parent }) => {
    const [paste, pasteStatus] = await getPaste(fetch, params.paste);

    if (!paste) {
        // TODO: error handling
        error(pasteStatus);
    }

    const relativeCreatedAt = formatDistanceToNow(new Date(paste.createdAt), { addSuffix: true });
    const relativeExpiresIn =
        paste.expiresIn !== ExpiresIn.never
            ? formatDistanceToNow(new Date(paste.deletesAt), { addSuffix: true })
            : "";
    const relativeEditedAt = paste.editedAt ? formatDistanceToNow(new Date(paste.editedAt), { addSuffix: true }) : undefined;
    const pasteStats = await getPasteStats(fetch, paste.id);
    const langStats = await getPasteLangs(fetch, paste.id);
    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];
    const isStarred = await isPasteStarred(fetch, paste.id);

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

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

    return {
        paste: paste,
        relativeCreatedAt: relativeCreatedAt,
        relativeEditedAt: relativeEditedAt,
        relativeExpiresIn: relativeExpiresIn,
        langStats: langStats,
        pasteStats: pasteStats,
        owner: owner,
        highlightedCode: highlightedCode,
        isStarred: isStarred
    };
};
