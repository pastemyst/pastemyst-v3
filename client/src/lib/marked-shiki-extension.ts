import type { MarkedExtension } from "marked";
import type { FetchFunc } from "./api/fetch";

export const markedShikiExtension = (fetch: FetchFunc, wrap: boolean, theme: string): MarkedExtension => {
    return {
        async: true,
        async walkTokens(token) {
            if (token.type === "code") {
                const res = await fetch("/internal/highlight", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        content: token.text,
                        language: token.lang,
                        wrap,
                        theme
                    })
                });

                token.raw = await res.text();
            }
        },
        renderer: {
            code({ raw }) {
                return raw;
            }
        }
    };
};
