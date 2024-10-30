<script lang="ts">
    import type { PageData } from "./$types";
    import { copyLinkToClipboardStore, currentUserStore } from "$lib/stores";
    import { onMount } from "svelte";
    import toast from "svelte-french-toast";
    import PasteHeader from "$lib/PasteHeader.svelte";
    import Pasties from "$lib/Pasties.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import { editPasteTags } from "$lib/api/paste";

    export let data: PageData;

    onMount(() => {
        if ($copyLinkToClipboardStore) {
            // TODO: maybe implement custom toast messages...
            toast.success("copied paste link to clipboard", {
                style: `
                    background-color: var(--color-bg);
                    border: 1px solid var(--color-bg2);
                    color: var(--color-fg);
                    border-radius: 0.2rem;
                    padding: 0.5rem;
                    font-size: 1rem;
                `
            });
            copyLinkToClipboardStore.set(false);
        }
    });

    const onUpdateTags = async () => {
        await editPasteTags(data.paste.id, data.paste.tags);
    };
</script>

<svelte:head>
    <title>pastemyst | {data.paste.title || "untitled"}</title>
</svelte:head>

<PasteHeader
    paste={data.paste}
    owner={data.owner}
    pasteStats={data.pasteStats}
    langStats={data.langStats}
    isStarred={data.isStarred}
/>

{#if $currentUserStore?.id === data.paste.ownerId && data.paste.tags}
    <TagInput bind:tags={data.paste.tags} existingTags={data.userTags} on:update={onUpdateTags} />
{/if}

<Pasties
    paste={data.paste}
    settings={data.settings}
    pasteStats={data.pasteStats}
    langStats={data.langStats}
    highlightedCode={data.highlightedCode}
    renderedMarkdown={data.renderedMarkdown}
/>
