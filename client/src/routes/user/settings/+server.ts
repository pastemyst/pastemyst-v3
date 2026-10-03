import { redirect, type RequestHandler } from "@sveltejs/kit";

// v2 settings page
export const GET: RequestHandler = async () => {
    redirect(301, "/settings");
};
