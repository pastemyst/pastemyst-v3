import type { LayoutServerLoad } from "./$types";
import { getSettings } from "$lib/api/settings";
import cookie from "cookie";
import { getSelf } from "$lib/api/auth";
import { getActivePastes, getVersion } from "$lib/api/meta";
import { getLatestAnnouncement } from "$lib/api/announcement";
import { marked } from "marked";

export const load: LayoutServerLoad = async ({ fetch, cookies, request }) => {
    const self = await getSelf(fetch, request.headers.get("cookie") ?? "");
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

    const [settings, settingsCookie] = await getSettings(fetch);

    if (settingsCookie) {
        const parsedSettingsCookie = cookie.parse(settingsCookie);

        if (parsedSettingsCookie["pastemyst_session_settings"]) {
            cookies.set(
                "pastemyst_session_settings",
                parsedSettingsCookie["pastemyst_session_settings"],
                {
                    path: parsedSettingsCookie.path!,
                    expires: new Date(parsedSettingsCookie.expires!),
                    sameSite: "strict"
                }
            );
        }
    }

    return {
        settings: settings,
        self: self,
        version: version,
        activePastes: activePastes,
        latestAnnouncement: latestAnnouncement,
        latestAnnouncementRendered: latestAnnouncementRendered
    };
};
