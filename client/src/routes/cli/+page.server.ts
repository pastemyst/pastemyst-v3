import { readFileSync } from "fs";
import type { PageServerLoad } from "./$types";
import path from "path";
import { marked } from "marked";
import { markedHeadingAnchorExtension } from "$lib/marked-heading-anchor";
import { markedShikiExtension } from "$lib/marked-shiki-extension";

export const load: PageServerLoad = async ({ fetch, parent }) => {
    const rawMarkdown = readFileSync(path.resolve("pastry.md"), "utf8");

    const { settings } = await parent();

    marked.use(markedHeadingAnchorExtension());
    marked.use(markedShikiExtension(fetch, settings.textWrap, settings.theme));
    const renderedMarkdown = await marked.parse(rawMarkdown, { gfm: true });

    return { renderedMarkdown };
};
