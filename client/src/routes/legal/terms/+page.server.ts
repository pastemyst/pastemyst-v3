import { resolve } from "path";
import type { PageServerLoad } from "./$types";
import { readFile } from "fs/promises";
import { marked } from "marked";

export const load: PageServerLoad = async () => {
    const termsOfServicePath = resolve("terms-of-service.md");
    const termsOfService = await readFile(termsOfServicePath, "utf-8");

    const renderedMarkdown = await marked.parse(termsOfService, { gfm: true });

    return {
        renderedMarkdown
    };
};
