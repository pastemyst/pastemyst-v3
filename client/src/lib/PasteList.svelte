<script lang="ts">
    import type { Page } from "./api/page";
    import { getUserPastes, type PasteWithLangStats } from "./api/paste";
    import type { User } from "./api/user";
    import PasteListItem from "./PasteListItem.svelte";

    export let pastes: Page<PasteWithLangStats>;
    export let user: User;
    export let pinned = false;

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
    {#each pastes.items as pasteWithLangStats}
        <PasteListItem {pasteWithLangStats} />
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
