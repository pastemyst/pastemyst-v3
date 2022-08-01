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
        <div class="flex row center">
            <h2>{user.username}</h2>

            <div class="badges flex row center">
                {#if user.contributor}
                    <div use:tooltip aria-label="contributor" class="flex">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            class="icon contributor"
                            viewBox="0 0 512 512"
                        >
                            <title>Rocket</title>
                            <path
                                fill="currentColor"
                                d="M328.85 156.79a26.69 26.69 0 1018.88 7.81 26.6 26.6 0 00-18.88-7.81z"
                            />
                            <path
                                fill="currentColor"
                                d="M477.44 50.06a.29.29 0 010-.09 20.4 20.4 0 00-15.13-15.3c-29.8-7.27-76.68.48-128.63 21.28-52.36 21-101.42 52-134.58 85.22A320.7 320.7 0 00169.55 175c-22.33-1-42 2.18-58.57 9.41-57.74 25.41-74.23 90.44-78.62 117.14a25 25 0 0027.19 29h.13l64.32-7.02c.08.82.17 1.57.24 2.26a34.36 34.36 0 009.9 20.72l31.39 31.41a34.27 34.27 0 0020.71 9.91l2.15.23-7 64.24v.13A25 25 0 00206 480a25.25 25.25 0 004.15-.34C237 475.34 302 459.05 327.34 401c7.17-16.46 10.34-36.05 9.45-58.34a314.78 314.78 0 0033.95-29.55c33.43-33.26 64.53-81.92 85.31-133.52 20.69-51.36 28.48-98.59 21.39-129.53zM370.38 224.94a58.77 58.77 0 110-83.07 58.3 58.3 0 010 83.07z"
                            />
                            <path
                                fill="currentColor"
                                d="M161.93 386.44a16 16 0 00-11 2.67c-6.39 4.37-12.81 8.69-19.29 12.9-13.11 8.52-28.79-6.44-21-20l12.15-21a16 16 0 00-15.16-24.91A61.25 61.25 0 0072 353.56c-3.66 3.67-14.79 14.81-20.78 57.26A357.94 357.94 0 0048 447.59 16 16 0 0064 464h.4a359.87 359.87 0 0036.8-3.2c42.47-6 53.61-17.14 57.27-20.8a60.49 60.49 0 0017.39-35.74 16 16 0 00-13.93-17.82z"
                            />
                        </svg>
                    </div>
                {/if}

                {#if user.supporter}
                    <div use:tooltip aria-label="supporter for {user.supporter} months" class="flex">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            class="icon supporter"
                            viewBox="0 0 512 512"
                        >
                            <title>Heart</title>
                            <path
                                fill="currentColor"
                                d="M256 448a32 32 0 01-18-5.57c-78.59-53.35-112.62-89.93-131.39-112.8-40-48.75-59.15-98.8-58.61-153C48.63 114.52 98.46 64 159.08 64c44.08 0 74.61 24.83 92.39 45.51a6 6 0 009.06 0C278.31 88.81 308.84 64 352.92 64c60.62 0 110.45 50.52 111.08 112.64.54 54.21-18.63 104.26-58.61 153-18.77 22.87-52.8 59.45-131.39 112.8a32 32 0 01-18 5.56z"
                            />
                        </svg>
                    </div>
                {/if}
            </div>
        </div>

        <p class="joined" use:tooltip aria-label={new Date(user.createdAt).toString()}>
            joined: {relativeJoined}
        </p>
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
                margin-right: 1rem;
                font-size: $fs-large;
                word-break: break-word;
                font-weight: normal;
            }

            .badges {
                .icon {
                    max-width: 18px;
                    margin-right: 0.5rem;

                    &.contributor {
                        color: $color-sec;
                    }

                    &.supporter {
                        color: $color-pink;
                    }
                }
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
