import { redirect, type RequestHandler } from "@sveltejs/kit";

// v2 cloned pastes at /clone/{id}; the home page does the cloning
export const GET: RequestHandler = ({ params }) => {
    redirect(302, `/?clone=${encodeURIComponent(params.paste!)}`);
};
