import { getCookie } from "../util/cookies";
import type { User } from "./user";

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
            Authorization: `Bearer ${token}`
        }
    });

    if (res.ok) {
        return (await res.json())["token"];
    } else {
        return null;
    }
};

export const getSelf = async (): Promise<User> => {
    const token = getCookie("pastemyst");

    if (token === null) return null;

    const res = await fetch(`/api/v3/auth/self`, {
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
        }
    });

    if (res.ok) {
        return await res.json();
    } else {
        return null;
    }
};
