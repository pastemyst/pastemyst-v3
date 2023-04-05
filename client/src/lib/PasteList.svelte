<script lang="ts">
    import moment from "moment";
    import type { Page } from "./api/page";
    import { ExpiresIn, getUserPastes, type Paste } from "./api/paste";
    import type { User } from "./api/user";
    import { tooltip } from "./tooltips";

    export let pastes: Page<Paste>;
    export let user: User;
    export let pinned = false;

    const getPasteLangs = (paste: Paste): string => {
        let langs: string[] = [];
        for (const pasty of paste.pasties) {
            if (!langs.some((l) => l === pasty.language)) {
                langs.push(pasty.language);
            }
        }

        return langs.slice(0, 3).join(", ");
    };

    const onPrevPage = async () => {
        if (pastes.currentPage === 0) return;

        const res = await getUserPastes(
            fetch,
            user.username,
            pinned,
            pastes.currentPage - 1,
            pastes.pageSize
        );

        if (res) pastes = res;
    };

    const onNextPage = async () => {
        if (pastes.currentPage === pastes.totalPages - 1) return;

        const res = await getUserPastes(
            fetch,
            user.username,
            pinned,
            pastes.currentPage + 1,
            pastes.pageSize
        );

        if (res) pastes = res;
    };
</script>

{#if pastes.items.length === 0}
    <p class="no-pastes">
        {user.username} doesn't have any {pinned ? "pinned" : "public"} pastes yet.
    </p>
{:else}
    {#each pastes.items as paste}
        <a href="/{paste.id}" class="paste btn">
            <div class="flex row center space-between">
                <p class="title">
                    {paste.title || "untitled"}
                    {#if paste.tags?.length > 0}
                        <span class="tags">{paste.tags.join(", ")}</span>
                    {/if}
                </p>

                <div>
                    {#if paste.private}
                        <div use:tooltip aria-label="private" class="flex">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 16 16"
                                class="icon"
                            >
                                <title>Lock Closed Icon</title>
                                <path
                                    fill="currentColor"
                                    fill-rule="evenodd"
                                    d="M4 4v2h-.25A1.75 1.75 0 002 7.75v5.5c0 .966.784 1.75 1.75 1.75h8.5A1.75 1.75 0 0014 13.25v-5.5A1.75 1.75 0 0012.25 6H12V4a4 4 0 10-8 0zm6.5 2V4a2.5 2.5 0 00-5 0v2h5zM12 7.5h.25a.25.25 0 01.25.25v5.5a.25.25 0 01-.25.25h-8.5a.25.25 0 01-.25-.25v-5.5a.25.25 0 01.25-.25H12z"
                                />
                            </svg>
                        </div>
                    {:else if paste.pinned}
                        <div use:tooltip aria-label="pinned" class="flex">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 16 16"
                                class="icon"
                            >
                                <title>Pin Icon</title>
                                <path
                                    fill="currentColor"
                                    fill-rule="evenodd"
                                    d="M4.456.734a1.75 1.75 0 012.826.504l.613 1.327a3.081 3.081 0 002.084 1.707l2.454.584c1.332.317 1.8 1.972.832 2.94L11.06 10l3.72 3.72a.75.75 0 11-1.061 1.06L10 11.06l-2.204 2.205c-.968.968-2.623.5-2.94-.832l-.584-2.454a3.081 3.081 0 00-1.707-2.084l-1.327-.613a1.75 1.75 0 01-.504-2.826L4.456.734zM5.92 1.866a.25.25 0 00-.404-.072L1.794 5.516a.25.25 0 00.072.404l1.328.613A4.582 4.582 0 015.73 9.63l.584 2.454a.25.25 0 00.42.12l5.47-5.47a.25.25 0 00-.12-.42L9.63 5.73a4.581 4.581 0 01-3.098-2.537L5.92 1.866z"
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

    {#if pastes.totalPages > 1}
        <div class="pager flex row center">
            <button class="btn" disabled={pastes.currentPage === 0} on:click={onPrevPage}>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Chevron Left</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M9.78 12.78a.75.75 0 01-1.06 0L4.47 8.53a.75.75 0 010-1.06l4.25-4.25a.75.75 0 011.06 1.06L6.06 8l3.72 3.72a.75.75 0 010 1.06z"
                    />
                </svg>
            </button>
            <span>{pastes.currentPage + 1}/{pastes.totalPages}</span>
            <button class="btn" disabled={!pastes.hasNextPage} on:click={onNextPage}>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Chevron Right</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M6.22 3.22a.75.75 0 011.06 0l4.25 4.25a.75.75 0 010 1.06l-4.25 4.25a.75.75 0 01-1.06-1.06L9.94 8 6.22 4.28a.75.75 0 010-1.06z"
                    />
                </svg>
            </button>
        </div>
    {/if}
{/if}

<style lang="scss">
    .no-pastes {
        text-align: center;
        margin: 0;
        font-size: $fs-normal;
    }

    .paste {
        display: block;
        background-color: var(--color-bg);
        margin-top: 1rem;
        border-radius: $border-radius;
        padding: 0.5rem;
        text-decoration: none;
        font-size: $fs-medium;
        border: 1px solid var(--color-bg2);
        color: var(--color-primary);

        &:hover {
            color: var(--color-secondary);
            background-color: var(--color-bg2);
            border-color: var(--color-bg3);
        }

        &:focus {
            color: var(--color-secondary);
            background-color: var(--color-bg2);
            border-color: var(--color-primary);
        }

        p {
            margin: 0;
        }

        .icon {
            color: var(--color-bg3);
        }

        span {
            font-size: $fs-small;
            color: var(--color-bg3);
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
</style>
