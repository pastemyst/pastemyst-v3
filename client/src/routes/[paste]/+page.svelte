<script lang="ts">
    import type { PageData } from "./$types";
    import { copyLinkToClipboardStore, currentUserStore } from "$lib/stores";
    import { onMount } from "svelte";
    import toast from "svelte-5-french-toast";
    import PasteHeader from "$lib/PasteHeader.svelte";
    import Pasties from "$lib/Pasties.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import { editPasteTags } from "$lib/api/paste";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let tags = $state(data.paste.tags);
    let paste = $state(data.paste);

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
        await editPasteTags(data.paste.id, tags);
    };
</script>

<svelte:head>
    <title>pastemyst | {data.paste.title || "untitled"}</title>
</svelte:head>

<PasteHeader
    bind:paste
    owner={data.owner || undefined}
    pasteStats={data.pasteStats || undefined}
    langStats={data.langStats}
    isStarred={data.isStarred}
/>

{#if $currentUserStore?.id === data.paste.ownerId && data.paste.tags}
    <TagInput
        bind:tags
        existingTags={data.userTags}
        onupdate={onUpdateTags}
        anonymousPaste={false}
        readonly={false}
    />
{/if}

<Pasties
    paste={data.paste}
    settings={data.settings}
    pasteStats={data.pasteStats || undefined}
    langStats={data.langStats}
    highlightedCode={data.highlightedCode}
    renderedMarkdown={data.renderedMarkdown}
/>
