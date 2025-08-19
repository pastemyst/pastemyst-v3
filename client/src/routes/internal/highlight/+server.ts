import { findLangByName } from "$lib/api/lang";
import { themes } from "$lib/themes";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import { readFileSync } from "fs";
import path from "path";
import { getSingletonHighlighter, type BundledLanguage, type LanguageRegistration } from "shiki";
import { grammars } from "tm-grammars";
import he from "he";

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(
        await highlight(json.content, json.wrap, json.theme, json.showLineNumbers, json.language)
    );
};

const highlight = async (
    content: string,
    wrap: boolean,
    theme: string,
    showLineNumbers: boolean,
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
                    g.scopeName === lang.tmScope ||
                    g.displayName.toLowerCase() === lang.name.toLowerCase() ||
                    g.name.toLowerCase() === lang.name.toLowerCase()
            );

            if (grammar) {
                const importPath = path.resolve(
                    `node_modules/tm-grammars/grammars/${grammar.name}.json`
                );
                const langJson: LanguageRegistration = JSON.parse(readFileSync(importPath, "utf8"));
                await highlighter.loadLanguage(langJson);

                actualLanguage = langJson.name;
            }
        }
    }

    if (!highlighter.getLoadedThemes().includes(themeJson["name"])) {
        await highlighter.loadTheme(themeJson);
    }

    const lines = content.split("\n").length;
    const maxDigits = lines.toString().length;

    const tokens = highlighter.codeToTokensWithThemes(content, {
        lang: actualLanguage as BundledLanguage,
        themes: { dark: themeJson["name"] }
    });

    const shikiTheme = highlighter.getTheme(themeJson["name"]);

    let result = `<pre class="shiki ${wrap && "wrap"}" style="background-color: ${shikiTheme.bg}"><code>`;

    for (const [i, line] of tokens.entries()) {
        let lineHtml = `<span class="line" data-line="${i + 1}">`;
        if (showLineNumbers) {
            lineHtml += `<span class="line-number" style="width: ${maxDigits}ch">${i + 1}</span>`;
        }
        for (const token of line) {
            lineHtml += `<span style="color: ${token.variants.dark.color}">${he.encode(token.content)}</span>`;
        }
        lineHtml += `</span>`;
        result += lineHtml;
    }

    result += "</code></pre>";

    return result;
};
