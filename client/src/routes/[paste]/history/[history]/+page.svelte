<script lang="ts">
    import PasteHeader from "$lib/PasteHeader.svelte";
    import Pasties from "$lib/Pasties.svelte";
    import { currentUserStore } from "$lib/stores";
    import TagInput from "$lib/TagInput.svelte";
    import { tooltip } from "$lib/tooltips";
    import type { PageData } from "./$types";

    export let data: PageData;
</script>

<PasteHeader paste={data.paste} owner={data.owner} pasteStats={data.pasteStats} langStats={data.langStats} isStarred={data.isStarred} />

{#if $currentUserStore?.id === data.paste.ownerId && data.paste.tags}
    <TagInput bind:tags={data.paste.tags} existingTags={[]} readonly />
{/if}

<section class="flex row center space-between">
    <p>you are viewing a past version of this paste, <a href="/{data.paste.id}">go back to the current version</a></p>

    <div class="flex row center gap-s">
	<a href={data.previousEdit ? `/${data.paste.id}/history/${data.previousEdit?.id}` : null} class="btn" aria-label="previous edit" use:tooltip class:disabled={!data.previousEdit}>
	    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
		<path
		    fill="currentColor"
		    d="M7.78 12.53a.75.75 0 0 1-1.06 0L2.47 8.28a.75.75 0 0 1 0-1.06l4.25-4.25a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042L4.81 7h7.44a.75.75 0 0 1 0 1.5H4.81l2.97 2.97a.75.75 0 0 1 0 1.06Z"
		/>
	    </svg>
	</a>
	<a href={data.nextEdit ? `/${data.paste.id}/history/${data.nextEdit?.id}` : null} class="btn" aria-label="next edit" use:tooltip class:disabled={!data.nextEdit}>
	    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
		<path
		    fill="currentColor"
		    d="M8.22 2.97a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042l2.97-2.97H3.75a.75.75 0 0 1 0-1.5h7.44L8.22 4.03a.75.75 0 0 1 0-1.06Z"
		/>
	    </svg>
	</a>
    </div>
</section>

<Pasties paste={data.paste} settings={data.settings} pasteStats={data.pasteStats} langStats={data.langStats} highlightedCode={data.highlightedCode} historyId={data.historyId} />

<style lang="scss">
    section {
        font-size: $fs-normal;
	margin: 1rem 0;
	border-color: var(--color-danger);

	p {
	    margin: 0;
	    margin-right: 2rem;
	}
    }
</style>
