import { getCookie } from "../util/cookies";

export const createAccount = async (username: string): Promise<string> => {
    const data = {
        username: username
    };

    const token = getCookie("pastemyst-registration");

    const res = await fetch(`/api/v3/auth/register`, {
        method: "POST",
        body: JSON.stringify(data),
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        }
    });

    if (res.ok) {
        return (await res.json())["token"];
    } else {
        return null;
    }
};