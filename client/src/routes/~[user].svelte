<script lang="ts" context="module">
    import { apiBase } from "$lib/api/api";
    import type { User } from "$lib/api/user";
    import moment from "moment";
    import { tooltip } from "$lib/tooltips";

    export const load = async ({ params, fetch }: { params: any; fetch: any }) => {
        const userRes = await fetch(`${apiBase}/user/${params.user}`, {
            method: "get"
        });

        const meRes = await fetch(`${apiBase}/auth/self`, {
            method: "get",
            credentials: "include"
        });

        let user: User;
        let relativeJoined: string;
        let isLoggedIn = false;
        if (userRes.ok) {
            user = await userRes.json();
            relativeJoined = moment(user.createdAt).fromNow();

            if (meRes.ok) {
                const loggedInUser: User = await meRes.json();

                isLoggedIn = loggedInUser.id === user.id;
            }
        }

        return {
            status: userRes.status,
            props: {
                user: user,
                isLoggedIn: isLoggedIn,
                relativeJoined: relativeJoined
            }
        };
    };
</script>

<script lang="ts">
    export let user: User;
    export let relativeJoined: string;
    export let isLoggedIn: boolean;
</script>

<section class="user-header flex row center">
    <img class="avatar" src={user.avatarUrl} alt="${user.username}'s avatar" />

    <div class="username flex col">
        <h2>{user.username}</h2>

        <p class="joined" use:tooltip aria-label="{new Date(user.createdAt).toString()}">joined: {relativeJoined}</p>
    </div>
</section>

<style lang="scss">
    .user-header {
        .avatar {
            display: inline-block;
            border-radius: $border-radius;
            max-width: 16%;
            height: auto;
            margin-right: 1rem;
        }

        .username {
            h2 {
                margin: 0;
                font-size: $fs-large;
                word-break: break-word;
                font-weight: normal;
            }

            .joined {
                font-size: $fs-small;
                color: $color-bg-3;
                margin: 0;
                margin-top: 0.25rem;
            }
        }
    }
</style>
