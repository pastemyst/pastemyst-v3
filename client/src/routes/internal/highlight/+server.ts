import { findLangByName } from "$lib/api/lang";
import { themes } from "$lib/themes";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import { readFileSync } from "fs";
import path from "path";
import { getSingletonHighlighter, type LanguageRegistration } from "shiki";
import { grammars } from "tm-grammars";

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(await highlight(json.content, json.wrap, json.theme, json.language));
};

const highlight = async (
    content: string,
    wrap: boolean,
    theme: string,
    language?: string
): Promise<string> => {
    const themeName = (themes.find((t) => t.name === theme) || themes[0]).shikiTheme;
    const themeJson = JSON.parse(readFileSync(`static/themes/${themeName}.json`, "utf8"));

    let actualLanguage: string = "text";

    const highlighter = await getSingletonHighlighter({
        themes: [],
        langs: []
    });

    if (language) {
        const lang = await findLangByName(fetch, language);

        if (lang && lang?.tmScope !== "none") {
            const grammar = grammars.find(
                (g) =>
                    g.scopeName === lang.tmScope || g.name.toLowerCase() === lang.name.toLowerCase()
            );

            if (grammar) {
                const importPath = path.resolve(
                    `node_modules/tm-grammars/grammars/${grammar.name}.json`
                );
                const langJson: LanguageRegistration = (
                    await import(importPath, { with: { type: "json" } })
                ).default;
                await highlighter.loadLanguage(langJson);

                actualLanguage = langJson.name;
            }
        }
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
