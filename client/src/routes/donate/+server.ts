import { redirect, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async () => {
    redirect(301, "/support");
};
