import { env } from "$env/dynamic/public";
import { redirect, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async () => {
    redirect(301, `${env.PUBLIC_DOCS_URL}/cli`);
};
