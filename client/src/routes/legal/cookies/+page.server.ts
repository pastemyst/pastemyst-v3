import { resolve } from "path";
import type { PageServerLoad } from "./$types";
import { readFile } from "fs/promises";
import { marked } from "marked";

export const prerender = false;

export const load: PageServerLoad = async () => {
    const cookiePolicyPath = resolve("cookie-policy.md");
    const cookiePolicy = await readFile(cookiePolicyPath, "utf-8");

    const renderedMarkdown = await marked.parse(cookiePolicy, { gfm: true });

    return {
        renderedMarkdown
    };
};
