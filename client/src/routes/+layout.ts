import { getLatestAnnouncement } from "$lib/api/announcement";
import { getSelf } from "$lib/api/auth";
import { getActivePastes, getVersion } from "$lib/api/meta";
import { marked } from "marked";
import type { LayoutLoad } from "./$types";

export const load: LayoutLoad = async ({ fetch, data }) => {
    const self = await getSelf(fetch);
    const version = await getVersion(fetch);
    const activePastes = await getActivePastes(fetch);
    let latestAnnouncement = await getLatestAnnouncement(fetch);
    const latestAnnouncementRendered = latestAnnouncement
        ? await marked.parse(latestAnnouncement.content, { gfm: true })
        : null;

    if (latestAnnouncement) {
        const latestAnnouncementDate = new Date(latestAnnouncement?.createdAt);
        // if the latest announcement is more than a week old don't show it
        if (Date.now() - latestAnnouncementDate.getTime() > 7 * 24 * 60 * 60 * 1000) {
            latestAnnouncement = undefined;
        }
    }

    return {
        self: self,
        version: version,
        activePastes: activePastes,
        settings: data.settings,
        latestAnnouncement: latestAnnouncement,
        latestAnnouncementRendered: latestAnnouncementRendered
    };
};
