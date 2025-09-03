import { getAllAnnouncements } from "$lib/api/announcement";
import { marked } from "marked";
import type { PageServerLoad } from "./$types";
import { error } from "@sveltejs/kit";

export const load: PageServerLoad = async ({ fetch, parent, request }) => {
    const { self } = await parent();

    if (!self) throw error(401);

    const announcements = await getAllAnnouncements(fetch, request.headers.get("cookie") ?? "");

    const renderedMarkdown: string[] = [];
    for (const announcement of announcements) {
        renderedMarkdown.push(await marked.parse(announcement.content, { gfm: true }));
    }

    return { announcements, renderedMarkdown };
};
