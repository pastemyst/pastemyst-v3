<script lang="ts">
    import { formatDistanceToNow } from "date-fns";
    import type { PageData } from "./$types";
    import { tooltip } from "$lib/tooltips";
    import PasteHeader from "$lib/PasteHeader.svelte";
    import { currentUserStore } from "$lib/stores";
    import TagInput from "$lib/TagInput.svelte";

    export let data: PageData;
</script>

<PasteHeader
    paste={data.paste}
    owner={data.owner}
    pasteStats={data.pasteStats}
    langStats={data.langStats}
    isStarred={data.isStarred}
/>

{#if $currentUserStore?.id === data.paste.ownerId && data.paste.tags}
    <TagInput
        bind:tags={data.paste.tags}
        existingTags={[]}
        anonymousPaste={false}
        readonly={true}
    />
{/if}

<section>
    <h3>
        list of all edits made to this paste, <a href="/{data.paste.id}">go back to the paste</a>
    </h3>

    {#each data.history as history}
        <div class="edit flex row center space-between">
            <div class="flex col">
                <a href="/{data.paste.id}/history/{history.id}/diff"
                    >{formatDistanceToNow(new Date(history.editedAt), { addSuffix: true })}</a
                >
                <span>{new Date(history.editedAt)}</span>
            </div>

            <div class="flex row center gap-s">
                <a
                    href="/{data.paste.id}/history/{history.id}/diff"
                    class="btn"
                    aria-label="view diff"
                    use:tooltip
                >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <path
                            fill="currentColor"
                            d="M8.75 1.75V5H12a.75.75 0 0 1 0 1.5H8.75v3.25a.75.75 0 0 1-1.5 0V6.5H4A.75.75 0 0 1 4 5h3.25V1.75a.75.75 0 0 1 1.5 0ZM4 13h8a.75.75 0 0 1 0 1.5H4A.75.75 0 0 1 4 13Z"
                        />
                    </svg>
                </a>

                <a
                    href="/{data.paste.id}/history/{history.id}"
                    class="btn"
                    aria-label="view paste at this point"
                    use:tooltip
                >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <path
                            fill="currentColor"
                            d="m11.28 3.22 4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.749.749 0 0 1-1.275-.326.749.749 0 0 1 .215-.734L13.94 8l-3.72-3.72a.749.749 0 0 1 .326-1.275.749.749 0 0 1 .734.215Zm-6.56 0a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042L2.06 8l3.72 3.72a.749.749 0 0 1-.326 1.275.749.749 0 0 1-.734-.215L.47 8.53a.75.75 0 0 1 0-1.06Z"
                        />
                    </svg>
                </a>
            </div>
        </div>
    {/each}
</section>

<style lang="scss">
    section {
        margin-top: 1rem;
    }

    h3 {
        font-weight: normal;
        font-size: $fs-normal;
        margin-bottom: 2rem;
    }

    .edit {
        margin-top: 1rem;
        background-color: var(--color-bg);
        padding: 0.5rem 1rem;
        border-radius: $border-radius;
        border: 1px solid var(--color-bg2);

        span {
            font-size: $fs-small;
            color: var(--color-bg3);
            margin-top: 0.25rem;
        }
    }
</style>
