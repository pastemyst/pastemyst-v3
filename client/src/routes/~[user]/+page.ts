import { getUserTags, type User } from "$lib/api/user";
import type { Page } from "$lib/api/page";
import type { PageLoad } from "./$types";
import type { PasteWithLangStats } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import { formatDistanceToNow } from "date-fns";
import { API_URL } from "$lib/api/fetch";

export const load: PageLoad = async ({ params, url, fetch }) => {
    const tag: string | null = url.searchParams.get("tag");
    const tagQuery = tag == null ? "" : "&tag=" + tag;

    const userRes = await fetch(`${API_URL}/users/${params.user}`, {
        method: "GET"
    });

    const meRes = await fetch(`${API_URL}/auth/self`, {
        method: "GET",
        credentials: "include"
    });

    const userPastesRes = await fetch(
        `${API_URL}/users/${params.user}/pastes?pageSize=5${tagQuery}`,
        {
            method: "GET",
            credentials: "include"
        }
    );

    const userPinnedPastesRes = await fetch(
        `${API_URL}/users/${params.user}/pastes/pinned?pageSize=5`,
        {
            method: "GET",
            credentials: "include"
        }
    );

    let user: User;
    let relativeJoined: string;
    let isCurrentUser = false;
    let pastes: Page<PasteWithLangStats>;
    let pinnedPastes: Page<PasteWithLangStats>;
    if (userRes.ok) {
        user = await userRes.json();
        relativeJoined = formatDistanceToNow(new Date(user.createdAt), { addSuffix: true });

        if (meRes.ok) {
            const loggedInUser: User = await meRes.json();

            isCurrentUser = loggedInUser.id === user.id;
        }

        if (userPastesRes.ok) {
            pastes = await userPastesRes.json();

            if (tag) {
                pinnedPastes = {
                    items: [],
                    totalPages: 0,
                    currentPage: 0,
                    hasNextPage: false,
                    pageSize: 5
                };
            } else {
                pinnedPastes = await userPinnedPastesRes.json();
            }
        } else {
            error(userPastesRes.status);
        }

        const tags = await getUserTags(fetch, user.username);

        return {
            user: user,
            isCurrentUser: isCurrentUser,
            relativeJoined: relativeJoined,
            pastes: pastes,
            pinnedPastes: pinnedPastes,
            tags: tags,
            tag: tag
        };
    } else {
        error(userRes.status);
    }
};
