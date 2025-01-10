<script lang="ts">
    import DiffEditor from "$lib/DiffEditor.svelte";
    import PasteHeader from "$lib/PasteHeader.svelte";
    import { currentUserStore } from "$lib/stores";
    import TagInput from "$lib/TagInput.svelte";
    import { tooltip } from "$lib/tooltips";
    import type { PageData } from "./$types";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();
</script>

<PasteHeader
    paste={data.paste}
    owner={data.owner || undefined}
    pasteStats={data.pasteStats || undefined}
    langStats={data.langStats}
    isStarred={data.isStarred}
/>

{#if $currentUserStore?.id === data.paste.ownerId && data.paste.tags}
    <TagInput bind:tags={data.paste.tags} existingTags={[]} readonly anonymousPaste={false} />
{/if}

<section class="flex row center space-between danger">
    <p>
        you are viewing the diff of the paste, <a href="/{data.paste.id}"
            >go back to the current version</a
        >
    </p>

    <div class="flex row center gap-s">
        <a
            href={data.previousEdit
                ? `/${data.paste.id}/history/${data.previousEdit?.id}/diff`
                : null}
            class="btn"
            aria-label="previous edit"
            use:tooltip
            class:disabled={!data.previousEdit}
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <path
                    fill="currentColor"
                    d="M7.78 12.53a.75.75 0 0 1-1.06 0L2.47 8.28a.75.75 0 0 1 0-1.06l4.25-4.25a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042L4.81 7h7.44a.75.75 0 0 1 0 1.5H4.81l2.97 2.97a.75.75 0 0 1 0 1.06Z"
                />
            </svg>
        </a>
        <a
            href={data.nextEdit ? `/${data.paste.id}/history/${data.nextEdit?.id}/diff` : null}
            class="btn"
            aria-label="next edit"
            use:tooltip
            class:disabled={!data.nextEdit}
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <path
                    fill="currentColor"
                    d="M8.22 2.97a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042l2.97-2.97H3.75a.75.75 0 0 1 0-1.5h7.44L8.22 4.03a.75.75 0 0 1 0-1.06Z"
                />
            </svg>
        </a>
    </div>
</section>

{#if data.oldTitle === data.newTitle && data.addedPasties.length === 0 && data.deletedPasties.length === 0 && data.modifiedPasties.length === 0}
    <section>
        <p>no changes</p>
    </section>
{/if}

{#if data.oldTitle !== data.newTitle}
    <section>
        <p>
            title: <code class="deleted">{data.oldTitle}</code> to
            <code class="added">{data.newTitle}</code>
        </p>
    </section>
{/if}

{#each data.addedPasties as addedPasty}
    <DiffEditor newPasty={addedPasty} settings={data.settings} />
{/each}

{#each data.deletedPasties as deletedPasty}
    <DiffEditor oldPasty={deletedPasty} settings={data.settings} />
{/each}

{#each data.modifiedPasties as { oldPasty, newPasty }}
    <DiffEditor {oldPasty} {newPasty} settings={data.settings} />
{/each}

<style lang="scss">
    section {
        font-size: $fs-normal;
        margin: 1rem 0;

        &.danger {
            border-color: var(--color-danger);
        }

        p {
            margin: 0;
            margin-right: 2rem;
        }
    }

    code.deleted {
        color: var(--color-danger);
    }

    code.added {
        color: var(--color-success);
    }
</style>
