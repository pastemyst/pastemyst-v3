import { error, redirect } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";

// Embed scripts used to live under /static/scripts (v2 under /static/scripts/libs). Embed snippets
// pasted on other sites still point there, so send them to where the files are now.
const iframeResizerFiles = ["iframeResizer.js", "iframeResizer.contentWindow.js"];

export const GET: RequestHandler = async ({ params }) => {
    const file = params.path.replace(/^libs\//, "");

    if (!iframeResizerFiles.includes(file)) error(404);

    redirect(301, `/scripts/${file}`);
};
