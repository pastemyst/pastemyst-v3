import { resolve } from "path";
import type { PageServerLoad } from "./$types";
import { readFile } from "fs/promises";
import { marked } from "marked";

export const prerender = false;

export const load: PageServerLoad = async () => {
    const copyrightPolicyPath = resolve("copyright-infringment-policy.md");
    const copyrightPolicy = await readFile(copyrightPolicyPath, "utf-8");

    const renderedMarkdown = await marked.parse(copyrightPolicy, { gfm: true });

    return {
        renderedMarkdown
    };
};