import { getPaste, getPasteLangs } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ fetch, params, request }) => {
    const cookieHeader = request.headers.get("cookie") ?? "";

    const [paste, pasteStatus] = await getPaste(fetch, params.paste, cookieHeader);

    if (!paste || pasteStatus !== 200) {
        error(pasteStatus);
    }

    const langStats = await getPasteLangs(fetch, paste.id, cookieHeader);

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
                wrap: true,
                theme: "myst"
            })
        });

        // TODO: error handling
        if (!res.ok) error(500);

        highlightedCode.push(await res.text());
    }

    return { paste, langStats, highlightedCode };
};
