import { getLangs } from "$lib/api/lang";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import { getHighlighter, type Highlighter, loadTheme, type ILanguageRegistration } from "shiki";
import { readFileSync } from "fs";

let highlighter: Highlighter;

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(await highlight(json.content, json.language));
};

const highlight = async (content: string, language: string) => {
    if (!highlighter) await initHighlighter();

    const lang = (await getLangs()).find(l => l.name === language);

    if (!lang || !lang.tmScope || lang.tmScope === "none") {
        return highlighter.codeToHtml(content, {});
    }

    return highlighter.codeToHtml(content, {
        lang: language
    });
};

const initHighlighter = async () => {
    const tomorrowmyst = await loadTheme("../../static/themes/tomorrowmyst.json");

    const langs = await getLangs();

    const shikiLangs: ILanguageRegistration[] = [];

    for (const lang of langs) {
        if (!lang.tmScope || lang.tmScope === "none") continue;

        const path = `./static/grammars/${lang.tmScope}.json`;

        // TODO: embedded langs?
        shikiLangs.push({
            id: lang.name,
            scopeName: lang.tmScope,
            aliases: lang.aliases,
            grammar: JSON.parse(readFileSync(path).toString()),
            path: path
        });
    }

    highlighter = await getHighlighter({
        theme: tomorrowmyst,
        langs: shikiLangs
    });
};
