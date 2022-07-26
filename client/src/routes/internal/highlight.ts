import type { RequestEvent } from "@sveltejs/kit";
import { getHighlighter, type Highlighter } from "shiki";

let highlighter: Highlighter;

export const POST = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return {
        status: 200,
        body: await highlight(json.content, json.language)
    };
};

const highlight = async (content: string, language: string) => {
    if (!highlighter) {
        highlighter = await getHighlighter({
            theme: "dracula"
        });
    }

    return highlighter.codeToHtml(content, {
        lang: language
    });
};
