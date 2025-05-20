<script lang="ts">
    import type { PageData } from "./$types";
    import { copyLinkToClipboardStore, currentUserStore } from "$lib/stores";
    import { onMount } from "svelte";
    import PasteHeader from "$lib/PasteHeader.svelte";
    import Pasties from "$lib/Pasties.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import { editPasteTags } from "$lib/api/paste";
    import { addToast } from "$lib/toasts.svelte";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let tags = $state(data.paste.tags);
    let paste = $state(data.paste);

    onMount(() => {
        if ($copyLinkToClipboardStore) {
            addToast("copied link to clipboard", "info");
            copyLinkToClipboardStore.set(false);
        }
    });

    const onUpdateTags = async () => {
        await editPasteTags(data.paste.id, tags);
    };
</script>

<svelte:head>
    <title>pastemyst | {data.paste.title || "untitled"}</title>
    <meta property="og:title" content="pastemyst | {data.paste.title || 'untitled'}" />
    <meta property="twitter:title" content="pastemyst | {data.paste.title || 'untitled'}" />
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
