import { getLangs } from "$lib/api/lang";
import type { RequestEvent } from "@sveltejs/kit";
import { getHighlighter, type Highlighter, loadTheme, type ILanguageRegistration } from "shiki";
import { readFileSync } from "fs";

let highlighter: Highlighter;

export const POST = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return {
        status: 200,
        body: await highlight(json.content, json.language)
    };
};

const highlight = async (content: string, language: string) => {
    if (!highlighter) await initHighlighter();

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

        // TODO: embedded langs?
        shikiLangs.push({
            id: lang.name,
            scopeName: lang.tmScope,
            aliases: lang.aliases,
            grammar: JSON.parse(
                readFileSync(`./static/grammars/${lang.tmScope}.json`).toString()
            )
        });
    }

    highlighter = await getHighlighter({
        theme: tomorrowmyst,
        langs: shikiLangs
    });
};
