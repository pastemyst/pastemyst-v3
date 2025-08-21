import { PUBLIC_DOCS_URL } from "$env/static/public";
import { redirect, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async () => {
    redirect(301, PUBLIC_DOCS_URL);
};
