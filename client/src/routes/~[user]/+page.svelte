<script lang="ts">
    import { apiBase } from "$lib/api/api";
    import { ExpiresIn, type Paste } from "$lib/api/paste";
    import type { PageData } from "./$types";
    import { tooltip } from "$lib/tooltips";
    import moment from "moment";

    export let data: PageData;

    const getPasteLangs = (paste: Paste): string => {
        let langs: string[] = [];
        for (const pasty of paste.pasties) {
            if (!langs.some((l) => l === pasty.language)) {
                langs.push(pasty.language);
            }
        }

        return langs.slice(0, 3).join(", ");
    };

    const fetchPastes = async (page: number) => {
        const res = await fetch(
            `${apiBase}/user/${data.user.username}/pastes?page=${page}&page_size=5`,
            {
                method: "get",
                credentials: "include"
            }
        );

        if (!res.ok) return;

        if (res.ok) {
            data.pastes = await res.json();
        }
    };

    const onPrevPage = async () => {
        if (data.pastes.page === 0) return;

        await fetchPastes(data.pastes.page - 1);
    };

    const onNextPage = async () => {
        if (data.pastes.page === data.pastes.totalPages - 1) return;

        await fetchPastes(data.pastes.page + 1);
    };
</script>

<svelte:head>
    <title>pastemyst | {data.user.username}</title>
</svelte:head>

<div class="flex sm-row">
    <section class="user-header flex sm-col center">
        <img class="avatar" src={data.user.avatarUrl} alt="${data.user.username}'s avatar" />

        <div class="username flex col">
            <div class="flex row center username-top">
                <h2>{data.user.username}</h2>

                <div class="badges flex row center">
                    {#if data.user.contributor}
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

                    {#if data.user.supporter}
                        <div
                            use:tooltip
                            aria-label="supporter for {data.user.supporter} months"
                            class="flex"
                        >
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

                    {#if data.isCurrentUser}
                        <a
                            href="/settings"
                            use:tooltip
                            aria-label="profile settings"
                            class="settings btn"
                        >
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                class="icon"
                                viewBox="0 0 512 512"
                            >
                                <title>Settings</title>
                                <circle fill="currentColor" cx="256" cy="256" r="48" />
                                <path
                                    fill="currentColor"
                                    d="M470.39 300l-.47-.38-31.56-24.75a16.11 16.11 0 01-6.1-13.33v-11.56a16 16 0 016.11-13.22L469.92 212l.47-.38a26.68 26.68 0 005.9-34.06l-42.71-73.9a1.59 1.59 0 01-.13-.22A26.86 26.86 0 00401 92.14l-.35.13-37.1 14.93a15.94 15.94 0 01-14.47-1.29q-4.92-3.1-10-5.86a15.94 15.94 0 01-8.19-11.82l-5.59-39.59-.12-.72A27.22 27.22 0 00298.76 26h-85.52a26.92 26.92 0 00-26.45 22.39l-.09.56-5.57 39.67a16 16 0 01-8.13 11.82 175.21 175.21 0 00-10 5.82 15.92 15.92 0 01-14.43 1.27l-37.13-15-.35-.14a26.87 26.87 0 00-32.48 11.34l-.13.22-42.77 73.95a26.71 26.71 0 005.9 34.1l.47.38 31.56 24.75a16.11 16.11 0 016.1 13.33v11.56a16 16 0 01-6.11 13.22L42.08 300l-.47.38a26.68 26.68 0 00-5.9 34.06l42.71 73.9a1.59 1.59 0 01.13.22 26.86 26.86 0 0032.45 11.3l.35-.13 37.07-14.93a15.94 15.94 0 0114.47 1.29q4.92 3.11 10 5.86a15.94 15.94 0 018.19 11.82l5.56 39.59.12.72A27.22 27.22 0 00213.24 486h85.52a26.92 26.92 0 0026.45-22.39l.09-.56 5.57-39.67a16 16 0 018.18-11.82c3.42-1.84 6.76-3.79 10-5.82a15.92 15.92 0 0114.43-1.27l37.13 14.95.35.14a26.85 26.85 0 0032.48-11.34 2.53 2.53 0 01.13-.22l42.71-73.89a26.7 26.7 0 00-5.89-34.11zm-134.48-40.24a80 80 0 11-83.66-83.67 80.21 80.21 0 0183.66 83.67z"
                                />
                            </svg>
                        </a>

                        <a
                            href="{apiBase}/auth/logout"
                            use:tooltip
                            aria-label="log out"
                            class="logout btn btn-danger"
                        >
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                class="icon"
                                viewBox="0 0 512 512"
                            >
                                <title>Log Out</title>
                                <path
                                    fill="currentColor"
                                    d="M160 256a16 16 0 0116-16h144V136c0-32-33.79-56-64-56H104a56.06 56.06 0 00-56 56v240a56.06 56.06 0 0056 56h160a56.06 56.06 0 0056-56V272H176a16 16 0 01-16-16zM459.31 244.69l-80-80a16 16 0 00-22.62 22.62L409.37 240H320v32h89.37l-52.68 52.69a16 16 0 1022.62 22.62l80-80a16 16 0 000-22.62z"
                                />
                            </svg>
                        </a>
                    {/if}
                </div>
            </div>

            <p class="joined" use:tooltip aria-label={new Date(data.user.createdAt).toString()}>
                joined: {data.relativeJoined}
            </p>
        </div>
    </section>

    <section class="public-pastes">
        <h3>public pastes</h3>

        {#if data.pastes.items.length === 0}
            <p class="no-public-pastes">{data.user.username} doesn't have any public pastes yet.</p>
        {:else}
            {#each data.pastes.items as paste}
                <a href="/{paste.id}" class="paste btn" sveltekit:prefetch>
                    <div class="flex row center space-between">
                        <p class="title">{paste.title === "" ? "(untitled)" : paste.title}</p>

                        <div>
                            {#if paste.private}
                                <div use:tooltip aria-label="private" class="flex">
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        class="icon"
                                        viewBox="0 0 512 512"
                                    >
                                        <title>Lock Closed</title>
                                        <path
                                            fill="currentColor"
                                            d="M368 192h-16v-80a96 96 0 10-192 0v80h-16a64.07 64.07 0 00-64 64v176a64.07 64.07 0 0064 64h224a64.07 64.07 0 0064-64V256a64.07 64.07 0 00-64-64zm-48 0H192v-80a64 64 0 11128 0z"
                                        />
                                    </svg>
                                </div>
                            {/if}
                        </div>
                    </div>

                    <div>
                        <!-- prettier-ignore -->
                        <span use:tooltip aria-label={new Date(paste.createdAt).toString()}>{moment(paste.createdAt).fromNow()}</span>

                        {#if paste.expiresIn !== ExpiresIn.never}
                            <!-- prettier-ignore -->
                            <span use:tooltip aria-label={new Date(paste.deletesAt).toString()}> - expires {moment(paste.deletesAt).fromNow()}</span>
                        {/if}
                    </div>

                    <div>
                        <span>{getPasteLangs(paste)}</span>
                    </div>
                </a>
            {/each}

            {#if data.pastes.totalPages > 1}
                <div class="pager flex row center">
                    <button class="btn" disabled={data.pastes.page === 0} on:click={onPrevPage}>
                        <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                            <title>Caret Back</title>
                            <path
                                fill="currentColor"
                                d="M321.94 98L158.82 237.78a24 24 0 000 36.44L321.94 414c15.57 13.34 39.62 2.28 39.62-18.22v-279.6c0-20.5-24.05-31.56-39.62-18.18z"
                            />
                        </svg>
                    </button>
                    <span>{data.pastes.page + 1}/{data.pastes.totalPages}</span>
                    <button class="btn" disabled={!data.pastes.hasNextPage} on:click={onNextPage}>
                        <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                            <title>Caret Forward</title>
                            <path
                                fill="currentColor"
                                d="M190.06 414l163.12-139.78a24 24 0 000-36.44L190.06 98c-15.57-13.34-39.62-2.28-39.62 18.22v279.6c0 20.5 24.05 31.56 39.62 18.18z"
                            />
                        </svg>
                    </button>
                </div>
            {/if}
        {/if}
    </section>
</div>

<style lang="scss">
    .user-header {
        margin-bottom: 0;
        height: 100%;

        .avatar {
            display: inline-block;
            border-radius: $border-radius;
            max-width: 16%;
            height: auto;
            margin-right: 1rem;
        }

        .username {
            width: 100%;

            h2 {
                margin: 0;
                margin-right: 1rem;
                font-size: $fs-large;
                word-break: break-word;
                font-weight: normal;
            }

            .username-top {
                width: 100%;
            }

            .badges {
                flex-grow: 1;

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

                .settings {
                    margin-left: auto;

                    .icon {
                        margin: 0;
                    }
                }

                .logout {
                    margin-left: 0.5rem;

                    .icon {
                        margin: 0;
                        color: $color-red;
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

    .public-pastes {
        flex-grow: 1;
        height: 100%;

        h3 {
            font-weight: normal;
            font-size: $fs-medium;
            margin: 0;
            margin-bottom: 1rem;
        }

        .no-public-pastes {
            text-align: center;
            margin: 0;
            font-size: $fs-normal;
        }

        .paste {
            display: block;
            background-color: $color-bg;
            margin-top: 1rem;
            border-radius: $border-radius;
            padding: 0.5rem;
            text-decoration: none;
            font-size: $fs-medium;
            border: 1px solid $color-bg-2;
            color: $color-prim;

            &:hover {
                color: $color-sec;
                background-color: $color-bg-2;
                border-color: $color-bg-3;
            }

            &:focus {
                color: $color-sec;
                background-color: $color-bg-2;
                border-color: $color-prim;
            }

            p {
                margin: 0;
            }

            .icon {
                max-width: 18px;
                max-height: 18px;
                color: $color-bg-3;
            }

            span {
                font-size: $fs-small;
                color: $color-bg-3;
            }
        }

        .pager {
            justify-content: center;
            margin-top: 1rem;
            font-size: $fs-normal;

            span {
                margin: 0 0.5rem;
            }

            button {
                .icon {
                    max-width: 18px;
                }
            }
        }
    }

    @media screen and (min-width: $break-med) {
        .user-header {
            min-width: 256px;
            max-width: 33.33%;
            margin-right: 2rem;

            .avatar {
                max-width: 100%;
                margin-right: 0;
                margin-bottom: 1rem;
            }
        }
    }
</style>
