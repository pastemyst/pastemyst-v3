import type { LayoutServerLoad } from "./$types";
import { getSettings } from "$lib/api/settings";
import cookie from "cookie";

export const load: LayoutServerLoad = async ({ fetch, cookies }) => {
    const [settings, settingsCookie] = await getSettings(fetch);

    if (settingsCookie) {
        const parsedSettingsCookie = cookie.parse(settingsCookie);

        cookies.set(
            "pastemyst_session_settings",
            parsedSettingsCookie["pastemyst_session_settings"],
            {
                path: parsedSettingsCookie.path,
                expires: new Date(parsedSettingsCookie.expires),
                sameSite: "strict"
            }
        );
    }

    return {
        settings: settings
    };
};
