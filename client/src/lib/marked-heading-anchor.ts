import type { MarkedExtension } from "marked";
import GithubSlugger from "github-slugger";
import sanitizeHtml from "sanitize-html";

const slugger = new GithubSlugger();

export const markedHeadingAnchorExtension = (): MarkedExtension => {
    return {
        hooks: {
            preprocess(src) {
                slugger.reset();
                return src;
            }
        },
        renderer: {
            heading({ tokens, depth }) {
                const text = this.parser.parseInline(tokens);

                const raw = sanitizeHtml(
                    this.parser.parseInline(tokens, this.parser.textRenderer)
                ).trim();

                const id = slugger.slug(raw.toLowerCase());

                return `
                    <div class="markdown-heading">
                        <h${depth} id="${id}">${text}</h${depth}>\n
                        <a class="markdown-heading-anchor" href="#${id}">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                                <path
                                    fill="currentColor"
                                    d="m7.775 3.275 1.25-1.25a3.5 3.5 0 1 1 4.95 4.95l-2.5 2.5a3.5 3.5 0 0 1-4.95 0 .751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018 1.998 1.998 0 0 0 2.83 0l2.5-2.5a2.002 2.002 0 0 0-2.83-2.83l-1.25 1.25a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042Zm-4.69 9.64a1.998 1.998 0 0 0 2.83 0l1.25-1.25a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042l-1.25 1.25a3.5 3.5 0 1 1-4.95-4.95l2.5-2.5a3.5 3.5 0 0 1 4.95 0 .751.751 0 0 1-.018 1.042.751.751 0 0 1-1.042.018 1.998 1.998 0 0 0-2.83 0l-2.5 2.5a1.998 1.998 0 0 0 0 2.83Z"
                                />
                            </svg>
                        </a>
                    </div>\n
                `;
            }
        }
    };
};
