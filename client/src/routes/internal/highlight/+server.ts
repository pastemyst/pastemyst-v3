import { getLangs } from "$lib/api/lang";
import type { RequestEvent, RequestHandler } from "@sveltejs/kit";
import {
    getHighlighter,
    type Highlighter,
    loadTheme,
    type ILanguageRegistration,
    type IThemedToken,
    type IShikiTheme
} from "shiki";
import { readFileSync } from "fs";

let highlighter: Highlighter;

export const POST: RequestHandler = async ({ request }: RequestEvent) => {
    const json = await request.json();

    return new Response(await highlight(json.content, json.language));
};

const highlight = async (content: string, language: string) => {
    if (!highlighter) await initHighlighter();

    const tomorrowmyst = await loadTheme("../../static/themes/tomorrowmyst.json");

    const lang = (await getLangs()).find((l) => l.name === language);

    let tokens: IThemedToken[][];

    if (!lang || !lang.tmScope || lang.tmScope === "none") {
        tokens = highlighter.codeToThemedTokens(content, "");
    } else {
        tokens = highlighter.codeToThemedTokens(content, language);
    }

    return tokensToHtml(tokens, tomorrowmyst);
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

const tokensToHtml = (tokens: IThemedToken[][], theme: IShikiTheme): string => {
    let res = `<div class="shiki" style="background-color: ${theme.bg}"><pre class="line-numbers"><code>`;

    for (let i = 0; i < tokens.length; i++) {
        res += `<span>${i + 1}</span>\n`;
    }

    res += `</code></pre>`;

    res += `<pre class="lines" tabindex="0"><code>`;

    for (const line of tokens) {
        let lineRes = `<span>`;
        for (const token of line) {
            lineRes += `<span style="color: ${token.color}">${escapeHtml(token.content)}</span>`;
        }
        lineRes += "</span>\n";

        res += lineRes;
    }

    res += `</code></pre></div>`;

    return res;
};

const escapeHtml = (html: string): string => {
    return html.replace(/[&<>"']/g, (c) => {
        switch (c) {
            case "&":
                return "&amp;";
            case "<":
                return "&lt;";
            case ">":
                return "&gt;";
            case '"':
                return "&quot;";
            case "'":
                return "&#39;";
        }

        return c;
    });
};
