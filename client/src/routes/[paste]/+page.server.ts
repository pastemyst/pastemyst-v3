import { error, redirect } from "@sveltejs/kit";
import {
    ExpiresIn,
    getPaste,
    getPasteLangs,
    getPasteStats,
    isPasteEncrypted,
    isPasteStarred
} from "$lib/api/paste";
import type { PageServerLoad } from "./$types";
import { getUserById, getUserTags } from "$lib/api/user";
import { formatDistanceToNow } from "date-fns";
import { isLanguageMarkdown } from "$lib/utils/markdown";
import { marked } from "marked";
import { markedHeadingAnchorExtension } from "$lib/marked-heading-anchor";
import { markedShikiExtension } from "$lib/marked-shiki-extension";

export const load: PageServerLoad = async ({ params, fetch, parent, request }) => {
    const cookieHeader = request.headers.get("cookie") ?? "";

    const [paste, pasteStatus] = await getPaste(fetch, params.paste, cookieHeader);

    if (!paste) {
        const isEncrypted = await isPasteEncrypted(fetch, params.paste, cookieHeader);

        if (isEncrypted) {
            redirect(302, `/${params.paste}/decrypt`);
        } else {
            // TODO: error handling
            error(pasteStatus);
        }
    }

    const relativeCreatedAt = formatDistanceToNow(new Date(paste.createdAt), { addSuffix: true });
    const relativeExpiresIn =
        paste.expiresIn !== ExpiresIn.never
            ? formatDistanceToNow(new Date(paste.deletesAt), { addSuffix: true })
            : "";
    const relativeEditedAt = paste.editedAt
        ? formatDistanceToNow(new Date(paste.editedAt), { addSuffix: true })
        : undefined;
    const pasteStats = await getPasteStats(fetch, paste.id, cookieHeader);
    const langStats = await getPasteLangs(fetch, paste.id, cookieHeader);
    const [owner, ownerStatus] =
        paste.ownerId !== null ? await getUserById(fetch, paste.ownerId) : [null, 0];
    const isStarred = await isPasteStarred(fetch, paste.id, cookieHeader);

    if (paste.ownerId !== null && !owner) {
        // TODO: error handling
        error(ownerStatus);
    }

    const { settings, self } = await parent();

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

    let tags: string[] = [];
    if (self) {
        tags = await getUserTags(fetch, self.username, cookieHeader);
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
        isStarred: isStarred,
        userTags: tags,
        renderedMarkdown: renderedMarkdown
    };
};
