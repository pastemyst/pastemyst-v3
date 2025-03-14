import { redirect, type RequestHandler } from "@sveltejs/kit";

export const GET: RequestHandler = async ({ params }) => {
    redirect(301, `/~${params.user}`);
};
