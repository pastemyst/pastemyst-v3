<script lang="ts">
    import { goto } from "$app/navigation";
    import { editPaste, type PasteEditInfo, type PastyEditInfo } from "$lib/api/paste";
    import { creatingPasteStore } from "$lib/stores";
    import TabbedEditor from "$lib/TabbedEditor.svelte";
    import type TabData from "$lib/TabData.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import type { PageData } from "./$types";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let tabs: TabData[] = $state([]);
    let activeTab: TabData | undefined = $state();

    const onEditPaste = async () => {
        let pasties: PastyEditInfo[] = [];

        for (const tab of tabs) {
            pasties.push({
                id: tab.id?.length === 8 ? tab.id : undefined, // TODO: maybe a better way to check if it's a new pasty
                title: tab.title!,
                content: tab.editor!.getContent(),
                language: tab.editor!.getSelectedLang()!.name
            });
        }

        const paste: PasteEditInfo = {
            title: data.paste.title,
            pasties
        };

        await editPaste(data.paste.id, paste);

        // TODO: handle if editing paste failed.

        $creatingPasteStore = true;

        goto(`/${data.paste.id}`);
    };
</script>

<svelte:head>
    <title>pastemyst | editing {data.paste.title || "untitled"}</title>
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

<TabbedEditor
    settings={data.settings}
    bind:tabs
    bind:activeTab
    existingPasties={data.paste.pasties}
/>

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
