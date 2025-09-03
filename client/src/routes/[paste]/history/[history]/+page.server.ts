import {
    getPasteAtEdit,
    getPasteHistoryCompact,
    getPasteLangs,
    getPasteStats,
    isPasteEncrypted,
    isPasteStarred
} from "$lib/api/paste";
import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { getUserById } from "$lib/api/user";
import { marked } from "marked";
import { markedHeadingAnchorExtension } from "$lib/marked-heading-anchor";
import { markedShikiExtension } from "$lib/marked-shiki-extension";
import { isLanguageMarkdown } from "$lib/utils/markdown";

export const load: PageServerLoad = async ({ params, fetch, parent, request }) => {
    const cookieHeader = request.headers.get("cookie") ?? "";

    const paste = await getPasteAtEdit(fetch, params.paste, params.history, cookieHeader);

    if (!paste) {
        const isEncrypted = await isPasteEncrypted(fetch, params.paste, cookieHeader);

        if (isEncrypted) {
            redirect(302, `/${params.paste}/decrypt`);
        } else {
            // TODO: error handling
            error(404);
        }
    }

    const history = await getPasteHistoryCompact(fetch, params.paste, cookieHeader);

    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

    const pasteStats = await getPasteStats(fetch, paste.id, cookieHeader);
    const langStats = await getPasteLangs(fetch, paste.id, cookieHeader);
    const isStarred = await isPasteStarred(fetch, paste.id, cookieHeader);

    const { settings } = await parent();

    const highlightedCode: string[] = [];

    for (const pasty of paste.pasties) {
        const res = await fetch("/internal/highlight", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                content: pasty.content,
                language: pasty.language,
                wrap: settings.textWrap,
                theme: settings.theme,
                showLineNumbers: true
            })
        });

        // TODO: error handling
        if (!res.ok) error(500);

        highlightedCode.push(await res.text());
    }

    marked.use(markedHeadingAnchorExtension());
    marked.use(markedShikiExtension(fetch, settings.textWrap, settings.theme));

    const renderedMarkdown: { id: string; renderedMarkdown: string }[] = [];
    for (const pasty of paste.pasties) {
        if (!isLanguageMarkdown(pasty.language)) continue;

        const rendered = await marked.parse(pasty.content, { gfm: true });

        renderedMarkdown.push({
            id: pasty.id,
            renderedMarkdown: rendered
        });
    }

    const currentHistoryIndex = history.findIndex((h) => h.id === params.history);

    type HistoryType = (typeof history)[0];

    const previousEdit: HistoryType | undefined = history[currentHistoryIndex + 1];
    const nextEdit: HistoryType | undefined = history[currentHistoryIndex - 1];

    return {
        paste,
        owner,
        pasteStats,
        langStats,
        isStarred,
        highlightedCode,
        previousEdit,
        nextEdit,
        renderedMarkdown,
        historyId: params.history
    };
};
