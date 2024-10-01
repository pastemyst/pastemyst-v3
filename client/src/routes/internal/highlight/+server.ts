import { getLangs } from "$lib/api/lang";
import { themes } from "$lib/themes";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import { readFileSync } from "fs";
import { getSingletonHighlighter, type LanguageRegistration } from "shiki";

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(await highlight(json.content, json.language, json.wrap, json.theme));
};

const highlight = async (
    content: string,
    language: string,
    wrap: boolean,
    theme: string
): Promise<string> => {
    const themeName = (themes.find((t) => t.name === theme) || themes[0]).shikiTheme;
    const themeJson = JSON.parse(readFileSync(`static/themes/${themeName}.json`, "utf8"));

    const langs = await getLangs(fetch);
    const lang = langs.find((l) => l.name === language);

    const highlighter = await getSingletonHighlighter({
        themes: [],
        langs: []
    });

    let actualLanguage: string = language;
    if (lang?.tmScope !== "none") {
        const langJson: LanguageRegistration = JSON.parse(
            readFileSync(`static/grammars/${lang?.tmScope}.json`, "utf8")
        );
        await highlighter.loadLanguage(langJson);

        actualLanguage = langJson.name;
    } else if (!lang || !lang.tmScope || lang.tmScope === "none") {
        actualLanguage = "text";
    }

    if (!highlighter.getLoadedThemes().includes(themeJson["name"])) {
        await highlighter.loadTheme(themeJson);
    }

    return highlighter.codeToHtml(content, {
        lang: actualLanguage,
        theme: themeJson["name"],
        transformers: [
            {
                pre(pre) {
                    if (wrap) this.addClassToHast(pre, "wrap");
                }
            }
        ]
    });
};
