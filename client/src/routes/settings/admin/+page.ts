import { getAllAnnouncements } from "$lib/api/announcement";
import { marked } from "marked";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
    const announcements = await getAllAnnouncements(fetch);

    const renderedMarkdown: string[] = [];
    for (const announcement of announcements) {
        renderedMarkdown.push(await marked.parse(announcement.content, { gfm: true }));
    }

    return { announcements, renderedMarkdown };
};
