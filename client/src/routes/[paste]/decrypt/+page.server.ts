import { type Actions, redirect } from "@sveltejs/kit";
import { env } from "$env/dynamic/public";

export const actions = {
    default: async (event) => {
        const pasteId = event.params.paste!;
        const data = await event.request.formData();
        const encryptionKey = data.get("encryptionKey");

        if (!encryptionKey) {
            return { success: false };
        }

        const res = await event.fetch(`${env.PUBLIC_API_BASE}/pastes/${pasteId}`, {
            method: "GET",
            credentials: "include",
            headers: {
                "Encryption-Key": encryptionKey.toString()
            }
        });

        if (!res.ok) {
            return { success: false };
        }

        const date = new Date();
        date.setTime(date.getTime() + 60 * 60 * 1000);

        event.cookies.set(`pastemyst-encryption-key-${pasteId}`, encryptionKey.toString(), {
            path: "/",
            httpOnly: true,
            sameSite: "strict",
            secure: env.PUBLIC_USE_HTTPS === "true",
            expires: date
        });

        redirect(302, `/${pasteId}`);
    }
} satisfies Actions;
