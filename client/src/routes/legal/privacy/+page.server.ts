import { resolve } from "path";
import type { PageServerLoad } from "./$types";
import { readFile } from "fs/promises";
import { marked } from "marked";

export const prerender = false;

export const load: PageServerLoad = async () => {
    const privacyPolicyPath = resolve("privacy-policy.md");
    const privacyPolicy = await readFile(privacyPolicyPath, "utf-8");

    const renderedMarkdown = await marked.parse(privacyPolicy, { gfm: true });

    return {
        renderedMarkdown
    };
};