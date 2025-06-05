<script lang="ts">
    import { goto } from "$app/navigation";
    import { editPaste, type PasteEditInfo, type PastyEditInfo } from "$lib/api/paste";
    import { creatingPasteStore } from "$lib/stores.svelte";
    import TabbedEditor from "$lib/TabbedEditor.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import { addToast } from "$lib/toasts.svelte";
    import type { PageData } from "./$types";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let editor: TabbedEditor;

    const onEditPaste = async () => {
        let pasties: PastyEditInfo[] = [];

        for (const tab of editor.getTabs()) {
            pasties.push({
                id: tab.id?.length === 8 ? tab.id : undefined, // TODO: maybe a better way to check if it's a new pasty
                title: tab.title!,
                content: tab.content,
                language: tab.language?.name ?? "Text"
            });
        }

        const paste: PasteEditInfo = {
            title: data.paste.title,
            pasties
        };

        const editPasteError = (await editPaste(data.paste.id, paste))[1];

        if (editPasteError) {
            addToast(`failed to edit paste: ${editPasteError.message}`, "error");
        } else {
            $creatingPasteStore = true;

            goto(`/${data.paste.id}`);
        }
    };
</script>

<svelte:head>
    <title>pastemyst | editing {data.paste.title || "untitled"}</title>
    <meta property="og:title" content="pastemyst | editing {data.paste.title || 'untitled'}" />
    <meta property="twitter:title" content="pastemyst | editing {data.paste.title || 'untitled'}" />
</svelte:head>

<div class="title-input flex sm-row">
    <label class="hidden" for="paste-title">paste title</label>
    <input
        type="text"
        placeholder="title"
        id="paste-title"
        name="paste-title"
        maxlength="128"
        autocomplete="off"
        bind:value={data.paste.title}
    />
</div>

<TagInput
    bind:tags={data.paste.tags}
    existingTags={data.userTags}
    anonymousPaste={false}
    readonly={true}
/>

<TabbedEditor settings={data.settings} existingPasties={data.paste.pasties} bind:this={editor} />

<div class="paste-options block flex row">
    <button class="btn-main" onclick={onEditPaste}>edit paste</button>
</div>

<style lang="scss">
    .title-input {
        margin-top: 2rem;
        margin-bottom: 1rem;

        input {
            width: 100%;
        }
    }

    .paste-options {
        margin-top: 2rem;
        padding: 0.5rem;
    }

    button {
        padding: 0.5rem 1rem;
        margin-left: auto;
    }
</style>
