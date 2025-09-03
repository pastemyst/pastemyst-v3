import { getReleases } from "$lib/api/meta";
import { marked } from "marked";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ fetch }) => {
    const releases = await getReleases(fetch);

    const renderedMarkdown: string[] = [];
    for (const release of releases) {
        renderedMarkdown.push(await marked.parse(release.content, { gfm: true }));
    }

    return {
        releases: releases,
        renderedMarkdown: renderedMarkdown
    };
};
