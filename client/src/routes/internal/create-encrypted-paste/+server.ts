import { error, json, type RequestEvent, type RequestHandler } from "@sveltejs/kit";
import { env } from "$env/dynamic/public";
import { type Paste, type PasteCreateInfo } from "$lib/api/paste";
import { getApiUrl } from "$lib/api/fetch";

type CreateInfoWithEncryptionKey = PasteCreateInfo & { encryptionKey: string };

export const POST: RequestHandler = async ({ request, fetch, cookies }: RequestEvent) => {
    const createInfo: CreateInfoWithEncryptionKey = await request.json();

    const res = await fetch(`${getApiUrl()}/pastes/`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
            "Encryption-Key": createInfo.encryptionKey
        },
        body: JSON.stringify(createInfo)
    });

    if (res.ok) {
        const paste: Paste = await res.json();
        const date = new Date();
        date.setTime(date.getTime() + 60 * 60 * 1000);

        cookies.set(`pastemyst-encryption-key-${paste.id}`, createInfo.encryptionKey, {
            path: "/",
            httpOnly: true,
            sameSite: "strict",
            secure: env.PUBLIC_USE_HTTPS === "true",
            expires: date
        });

        return json(paste);
    }

    return error(res.status);
};
