import { getLangs } from "$lib/api/lang";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import { readFileSync } from "fs";
import { getHighlighter } from "shiki";

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(await highlight(json.content, json.language));
};

const highlight = async (content: string, language: string): Promise<string> => {
    const tomorrowmyst = JSON.parse(readFileSync("static/themes/tomorrowmyst.json", "utf8"));

    const langs = await getLangs(fetch);
    const lang = langs.find((l) => l.name === language);

    const langJson = JSON.parse(readFileSync(`static/grammars/${lang?.tmScope}.json`, "utf8"));

    const highlighter = await getHighlighter({
        themes: [],
        langs: []
    });

    await highlighter.loadLanguage(langJson);
    await highlighter.loadTheme(tomorrowmyst);

    let actualLanguage = language;
    if (!lang || !lang.tmScope || lang.tmScope === "none") {
        actualLanguage = "text";
    }

    return highlighter.codeToHtml(content, {
        lang: actualLanguage,
        theme: "TomorrowMyst"
    });
};
