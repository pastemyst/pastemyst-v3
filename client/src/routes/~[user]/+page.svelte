<script lang="ts">
    import type { PageData } from "./$types";
    import { tooltip } from "$lib/tooltips";
    import PasteList from "$lib/PasteList.svelte";
    import { cmdPalOpen, cmdPalTitle, currentUserStore } from "$lib/stores.svelte";
    import { Close, getConfirmActionCommands, setTempCommands } from "$lib/command";
    import { deleteUser } from "$lib/api/user";
    import { getApiUrl } from "$lib/api/fetch";
    import { env } from "$env/dynamic/public";

    interface Props {
        data: PageData;
    }

    let { data }: Props = $props();

    const onDeleteUser = () => {
        setTempCommands(
            getConfirmActionCommands(() => {
                (async () => {
                    const ok = await deleteUser(data.user.username);

                    if (!ok) {
                        // TODO: nicer error handling
                        alert("failed to delete user. try again later.");
                    }

                    window.location.href = "/";
                })();

                return Close.yes;
            })
        );

        cmdPalTitle.set(
            "are you sure you want to delete this user? this will delete the user and all the associated data, including the pastes"
        );
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst | ~{data.user.username}</title>
    <meta property="og:title" content="pastemyst | ~{data.user.username}" />
    <meta property="twitter:title" content="pastemyst | ~{data.user.username}" />
</svelte:head>

<div class="flex sm-row">
    <div class="left flex col">
        <section class="user-header">
            <img
                class="avatar"
                src="{env.PUBLIC_API_CLIENT_BASE}/images/{data.user.avatarId}"
                alt="${data.user.username}'s avatar"
            />

            <div class="username flex col">
                <div class="flex row center username-top">
                    <h2>{data.user.username}</h2>

                    <div class="badges flex row center">
                        {#if data.user.isContributor}
                            <div use:tooltip aria-label="contributor" class="flex">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon contributor"
                                >
                                    <title>Rocket Icon</title>
                                    <path
                                        fill="currentColor"
                                        fill-rule="evenodd"
                                        d="M14.064 0a8.75 8.75 0 00-6.187 2.563l-.459.458c-.314.314-.616.641-.904.979H3.31a1.75 1.75 0 00-1.49.833L.11 7.607a.75.75 0 00.418 1.11l3.102.954c.037.051.079.1.124.145l2.429 2.428c.046.046.094.088.145.125l.954 3.102a.75.75 0 001.11.418l2.774-1.707a1.75 1.75 0 00.833-1.49V9.485c.338-.288.665-.59.979-.904l.458-.459A8.75 8.75 0 0016 1.936V1.75A1.75 1.75 0 0014.25 0h-.186zM10.5 10.625c-.088.06-.177.118-.266.175l-2.35 1.521.548 1.783 1.949-1.2a.25.25 0 00.119-.213v-2.066zM3.678 8.116L5.2 5.766c.058-.09.117-.178.176-.266H3.309a.25.25 0 00-.213.119l-1.2 1.95 1.782.547zm5.26-4.493A7.25 7.25 0 0114.063 1.5h.186a.25.25 0 01.25.25v.186a7.25 7.25 0 01-2.123 5.127l-.459.458a15.21 15.21 0 01-2.499 2.02l-2.317 1.5-2.143-2.143 1.5-2.317a15.25 15.25 0 012.02-2.5l.458-.458h.002zM12 5a1 1 0 11-2 0 1 1 0 012 0zm-8.44 9.56a1.5 1.5 0 10-2.12-2.12c-.734.73-1.047 2.332-1.15 3.003a.23.23 0 00.265.265c.671-.103 2.273-.416 3.005-1.148z"
                                    />
                                </svg>
                            </div>
                        {/if}

                        {#if data.user.isSupporter}
                            <div use:tooltip aria-label="supporter" class="flex">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon supporter"
                                >
                                    <title>Heart Icon</title>
                                    <path
                                        fill="currentColor"
                                        d="m8 14.25.345.666a.75.75 0 0 1-.69 0l-.008-.004-.018-.01a7.152 7.152 0 0 1-.31-.17 22.055 22.055 0 0 1-3.434-2.414C2.045 10.731 0 8.35 0 5.5 0 2.836 2.086 1 4.25 1 5.797 1 7.153 1.802 8 3.02 8.847 1.802 10.203 1 11.75 1 13.914 1 16 2.836 16 5.5c0 2.85-2.045 5.231-3.885 6.818a22.066 22.066 0 0 1-3.744 2.584l-.018.01-.006.003h-.002ZM4.25 2.5c-1.336 0-2.75 1.164-2.75 3 0 2.15 1.58 4.144 3.365 5.682A20.58 20.58 0 0 0 8 13.393a20.58 20.58 0 0 0 3.135-2.211C12.92 9.644 14.5 7.65 14.5 5.5c0-1.836-1.414-3-2.75-3-1.373 0-2.609.986-3.029 2.456a.749.749 0 0 1-1.442 0C6.859 3.486 5.623 2.5 4.25 2.5Z"
                                    />
                                </svg>
                            </div>
                        {/if}

                        {#if data.user.isAdmin}
                            <div use:tooltip aria-label="admin" class="flex">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon admin"
                                >
                                    <title>Admin Icon</title>
                                    <path
                                        fill="currentColor"
                                        d="M5.433 2.304A4.492 4.492 0 0 0 3.5 6c0 1.598.832 3.002 2.09 3.802.518.328.929.923.902 1.64v.008l-.164 3.337a.75.75 0 1 1-1.498-.073l.163-3.33c.002-.085-.05-.216-.207-.316A5.996 5.996 0 0 1 2 6a5.993 5.993 0 0 1 2.567-4.92 1.482 1.482 0 0 1 1.673-.04c.462.296.76.827.76 1.423v2.82c0 .082.041.16.11.206l.75.51a.25.25 0 0 0 .28 0l.75-.51A.249.249 0 0 0 9 5.282V2.463c0-.596.298-1.127.76-1.423a1.482 1.482 0 0 1 1.673.04A5.993 5.993 0 0 1 14 6a5.996 5.996 0 0 1-2.786 5.068c-.157.1-.209.23-.207.315l.163 3.33a.752.752 0 0 1-1.094.714.75.75 0 0 1-.404-.64l-.164-3.345c-.027-.717.384-1.312.902-1.64A4.495 4.495 0 0 0 12.5 6a4.492 4.492 0 0 0-1.933-3.696c-.024.017-.067.067-.067.16v2.818a1.75 1.75 0 0 1-.767 1.448l-.75.51a1.75 1.75 0 0 1-1.966 0l-.75-.51A1.75 1.75 0 0 1 5.5 5.282V2.463c0-.092-.043-.142-.067-.159Z"
                                    />
                                </svg>
                            </div>
                        {/if}

                        {#if data.isCurrentUser}
                            <a
                                href="/settings/profile"
                                use:tooltip
                                aria-label="profile settings"
                                class="settings btn"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon"
                                >
                                    <title>Gear Icon</title>
                                    <path
                                        fill="currentColor"
                                        fill-rule="evenodd"
                                        d="M7.429 1.525a6.593 6.593 0 011.142 0c.036.003.108.036.137.146l.289 1.105c.147.56.55.967.997 1.189.174.086.341.183.501.29.417.278.97.423 1.53.27l1.102-.303c.11-.03.175.016.195.046.219.31.41.641.573.989.014.031.022.11-.059.19l-.815.806c-.411.406-.562.957-.53 1.456a4.588 4.588 0 010 .582c-.032.499.119 1.05.53 1.456l.815.806c.08.08.073.159.059.19a6.494 6.494 0 01-.573.99c-.02.029-.086.074-.195.045l-1.103-.303c-.559-.153-1.112-.008-1.529.27-.16.107-.327.204-.5.29-.449.222-.851.628-.998 1.189l-.289 1.105c-.029.11-.101.143-.137.146a6.613 6.613 0 01-1.142 0c-.036-.003-.108-.037-.137-.146l-.289-1.105c-.147-.56-.55-.967-.997-1.189a4.502 4.502 0 01-.501-.29c-.417-.278-.97-.423-1.53-.27l-1.102.303c-.11.03-.175-.016-.195-.046a6.492 6.492 0 01-.573-.989c-.014-.031-.022-.11.059-.19l.815-.806c.411-.406.562-.957.53-1.456a4.587 4.587 0 010-.582c.032-.499-.119-1.05-.53-1.456l-.815-.806c-.08-.08-.073-.159-.059-.19a6.44 6.44 0 01.573-.99c.02-.029.086-.075.195-.045l1.103.303c.559.153 1.112.008 1.529-.27.16-.107.327-.204.5-.29.449-.222.851-.628.998-1.189l.289-1.105c.029-.11.101-.143.137-.146zM8 0c-.236 0-.47.01-.701.03-.743.065-1.29.615-1.458 1.261l-.29 1.106c-.017.066-.078.158-.211.224a5.994 5.994 0 00-.668.386c-.123.082-.233.09-.3.071L3.27 2.776c-.644-.177-1.392.02-1.82.63a7.977 7.977 0 00-.704 1.217c-.315.675-.111 1.422.363 1.891l.815.806c.05.048.098.147.088.294a6.084 6.084 0 000 .772c.01.147-.038.246-.088.294l-.815.806c-.474.469-.678 1.216-.363 1.891.2.428.436.835.704 1.218.428.609 1.176.806 1.82.63l1.103-.303c.066-.019.176-.011.299.071.213.143.436.272.668.386.133.066.194.158.212.224l.289 1.106c.169.646.715 1.196 1.458 1.26a8.094 8.094 0 001.402 0c.743-.064 1.29-.614 1.458-1.26l.29-1.106c.017-.066.078-.158.211-.224a5.98 5.98 0 00.668-.386c.123-.082.233-.09.3-.071l1.102.302c.644.177 1.392-.02 1.82-.63.268-.382.505-.789.704-1.217.315-.675.111-1.422-.364-1.891l-.814-.806c-.05-.048-.098-.147-.088-.294a6.1 6.1 0 000-.772c-.01-.147.039-.246.088-.294l.814-.806c.475-.469.679-1.216.364-1.891a7.992 7.992 0 00-.704-1.218c-.428-.609-1.176-.806-1.82-.63l-1.103.303c-.066.019-.176.011-.299-.071a5.991 5.991 0 00-.668-.386c-.133-.066-.194-.158-.212-.224L10.16 1.29C9.99.645 9.444.095 8.701.031A8.094 8.094 0 008 0zm1.5 8a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM11 8a3 3 0 11-6 0 3 3 0 016 0z"
                                    />
                                </svg>
                            </a>

                            <a
                                href="{getApiUrl()}/auth/logout"
                                use:tooltip
                                aria-label="log out"
                                class="logout btn btn-danger"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon"
                                >
                                    <title>Sign Out Icon</title>
                                    <path
                                        fill="currentColor"
                                        fill-rule="evenodd"
                                        d="M2 2.75C2 1.784 2.784 1 3.75 1h2.5a.75.75 0 010 1.5h-2.5a.25.25 0 00-.25.25v10.5c0 .138.112.25.25.25h2.5a.75.75 0 010 1.5h-2.5A1.75 1.75 0 012 13.25V2.75zm10.44 4.5H6.75a.75.75 0 000 1.5h5.69l-1.97 1.97a.75.75 0 101.06 1.06l3.25-3.25a.75.75 0 000-1.06l-3.25-3.25a.75.75 0 10-1.06 1.06l1.97 1.97z"
                                    />
                                </svg>
                            </a>
                        {/if}
                    </div>
                </div>

                <p class="joined" use:tooltip aria-label={new Date(data.user.createdAt).toString()}>
                    joined: {data.relativeJoined}
                </p>

                {#if $currentUserStore && $currentUserStore.isAdmin && !data.isCurrentUser}
                    <button class="btn-danger delete-user-btn" onclick={onDeleteUser}
                        >delete user</button
                    >
                {/if}
            </div>
        </section>

        {#if data.tags?.length > 0}
            <details class="tags" open={data.tag !== null}>
                <summary class="flex row center">
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 16 16"
                        class="icon chevron"
                    >
                        <path
                            fill="currentColor"
                            d="M6.22 3.22a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042L9.94 8 6.22 4.28a.75.75 0 0 1 0-1.06Z"
                        />
                    </svg>
                    <span class="summary-title">tags</span>

                    {#if data.tag}
                        <a
                            href="/~{data.self?.username}"
                            class="btn btn-icon clear-tag-filter btn-danger"
                            aria-label="clear the tag filter"
                            use:tooltip
                        >
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 16 16"
                                class="icon"
                            >
                                <path
                                    fill="currentColor"
                                    d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.749.749 0 0 1 1.275.326.749.749 0 0 1-.215.734L9.06 8l3.22 3.22a.749.749 0 0 1-.326 1.275.749.749 0 0 1-.734-.215L8 9.06l-3.22 3.22a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"
                                />
                            </svg>
                        </a>
                    {/if}
                </summary>

                <div class="tags-wrapper">
                    {#each data.tags as tag}
                        <a
                            href="/~{data.self?.username}?tag={tag}"
                            class="btn"
                            class:active={tag == data.tag}>{tag}</a
                        >
                    {/each}
                </div>
            </details>
        {/if}
    </div>

    <div class="pastes">
        {#if data.pinnedPastes.items.length > 0}
            <section>
                <h3>pinned</h3>

                <PasteList pastes={data.pinnedPastes} user={data.user} pinned />
            </section>
        {/if}

        {#if data.pinnedPastes.items.length == 0 || data.pastes.items.length > 0}
            <section>
                <h3>pastes</h3>

                <PasteList pastes={data.pastes} user={data.user} />
            </section>
        {/if}
    </div>
</div>

<style lang="scss">
    .left {
        width: 100%;
    }

    .user-header {
        margin-bottom: 0;
        width: 100%;

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
                margin-right: 0.5rem;
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
                    margin-right: 0.75rem;

                    &.contributor {
                        color: var(--color-secondary);
                    }

                    &.supporter {
                        color: var(--color-pink);
                    }

                    &.admin {
                        color: var(--color-danger);
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
                        color: var(--color-danger);
                    }
                }
            }

            .joined {
                font-size: $fs-small;
                color: var(--color-bg3);
                margin: 0;
                margin-top: 0.25rem;
            }

            .delete-user-btn {
                margin-top: 1rem;
            }
        }
    }

    .tags {
        margin-bottom: 0;
        width: 100%;
        background-color: var(--color-bg1);
        border: 1px solid var(--color-bg2);
        margin-top: 2rem;
        padding: 0.5rem 1rem;
        border-radius: $border-radius;
        cursor: pointer;

        &[open] .icon {
            transform: rotate(90deg);
        }

        summary {
            list-style: none;
            user-select: none;

            &::-webkit-details-marker {
                display: none;
            }

            .chevron {
                margin-right: 0.5rem;
                @include transition(transform);
            }

            .clear-tag-filter {
                margin-top: 0;
                margin-left: auto;
                padding: 0;
            }
        }

        .tags-wrapper {
            margin-top: 1rem;
        }

        a {
            margin-top: 0.5rem;
            background-color: var(--color-bg);

            &.active {
                border-color: var(--color-primary);
            }
        }
    }

    .pastes {
        flex-grow: 1;
        height: 100%;

        h3 {
            font-weight: normal;
            font-size: $fs-medium;
            margin: 0;
            margin-bottom: 1rem;
        }
    }

    @media screen and (min-width: $break-med) {
        .left {
            min-width: 256px;
            max-width: 33.33%;
            margin-right: 2rem;
            width: min-content;

            .avatar {
                max-width: 100%;
                margin-right: 0;
                margin-bottom: 1rem;
            }
        }
    }
</style>
