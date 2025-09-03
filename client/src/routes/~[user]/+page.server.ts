import { getUserTags, type User } from "$lib/api/user";
import type { Page } from "$lib/api/page";
import type { PageServerLoad } from "./$types";
import type { PasteWithLangStats } from "$lib/api/paste";
import { error } from "@sveltejs/kit";
import { formatDistanceToNow } from "date-fns";
import { getApiUrl } from "$lib/api/fetch";

export const load: PageServerLoad = async ({ params, url, fetch, parent, request }) => {
    const tag: string | null = url.searchParams.get("tag");
    const tagQuery = tag == null ? "" : "&tag=" + tag;

    const userRes = await fetch(`${getApiUrl()}/users/${params.user}`, {
        method: "GET"
    });

    const { self } = await parent();

    const cookie = request.headers.get("cookie") ?? "";

    const userPastesRes = await fetch(
        `${getApiUrl()}/users/${params.user}/pastes?pageSize=5${tagQuery}`,
        {
            method: "GET",
            credentials: "include",
            headers: {
                cookie
            }
        }
    );

    const userPinnedPastesRes = await fetch(
        `${getApiUrl()}/users/${params.user}/pastes/pinned?pageSize=5`,
        {
            method: "GET",
            credentials: "include",
            headers: {
                cookie
            }
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

        if (self) {
            isCurrentUser = self.id === user.id;
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

        const tags = await getUserTags(fetch, user.username, cookie);

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
