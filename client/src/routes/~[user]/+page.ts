import { getUserTags, type User } from "$lib/api/user";
import moment from "moment";
import type { Page } from "$lib/api/page";
import type { PageLoad } from "./$types";
import type { PasteWithLangStats } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import { PUBLIC_API_BASE } from "$env/static/public";

export const load: PageLoad = async ({ params, url, fetch }) => {
    const tag: string | null = url.searchParams.get("tag");
    const tagQuery = tag == null ? "" : "&tag=" + tag;

    const userRes = await fetch(`${PUBLIC_API_BASE}/users/${params.user}`, {
        method: "get"
    });

    const meRes = await fetch(`${PUBLIC_API_BASE}/auth/self`, {
        method: "get",
        credentials: "include"
    });

    const userPastesRes = await fetch(
        `${PUBLIC_API_BASE}/users/${params.user}/pastes?pageSize=5${tagQuery}`,
        {
            method: "get",
            credentials: "include"
        }
    );

    const userPinnedPastesRes = await fetch(
        `${PUBLIC_API_BASE}/users/${params.user}/pastes/pinned?pageSize=5`,
        {
            method: "get",
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
        relativeJoined = moment(user.createdAt).fromNow();

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
            throw error(userPastesRes.status);
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
        throw error(userRes.status);
    }
};
