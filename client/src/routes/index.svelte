<script lang="ts">
    import { goto } from "$app/navigation";
    import { createPaste, PasteSkeleton, PastySkeleton } from "$lib/api/paste";
    import PasteOptions from "$lib/PasteOptions.svelte";
    import TabbedEditor from "$lib/TabbedEditor.svelte";
    import type TabData from "$lib/TabData";

    let expiresIn = "never";

    let title: string;

    let tabs: TabData[];

    const onCreatePaste = async () => {
        let pasties: PastySkeleton[] = [];

        for (const tab of tabs) {
            pasties.push({
                title: tab.title,
                content: tab.editor.getContent()
            });
        }

        const pasteSkeleton: PasteSkeleton = {
            title: title,
            pasties: pasties
        };

        const paste = await createPaste(pasteSkeleton);

        goto(`/${paste._id}`);
    };
</script>

<svelte:head>
    <title>pastemyst | home</title>
</svelte:head>

<div class="title-input flex sm-row">
    <label class="hidden" for="paste-title">paste title</label>
    <input type="text" placeholder="title" id="paste-title" name="paste-title" maxlength="128" bind:value={title} />

    <button>expires in: {expiresIn}</button>
</div>

<TabbedEditor bind:tabs />

<div class="paste-options">
    <PasteOptions on:create={onCreatePaste} />
</div>

<style lang="scss">
    .title-input {
        margin-top: 2rem;
        margin-bottom: 2rem;

        input {
            width: 100%;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }

        button {
            border-top-left-radius: 0;
            border-bottom-left-radius: 0;
            white-space: nowrap;
            padding: 0.5rem 1rem;
            border-left-color: $color-bg-1;

            &:hover {
                border-left-color: $color-bg-3;
            }

            &:focus,
            &:active {
                border-left-color: $color-prim;
            }
        }

        @media screen and (max-width: $break-med) {
            input {
                border-radius: $border-radius;
                border-bottom-left-radius: 0;
                border-bottom-right-radius: 0;
            }

            button {
                border-radius: $border-radius;
                border-top-left-radius: 0;
                border-top-right-radius: 0;
                text-align: left;
                border-left-color: $color-bg-2;
                border-top-color: $color-bg-1;

                &:hover {
                    border-top-color: $color-bg-3;
                }

                &:focus,
                &:active {
                    border-top-color: $color-prim;
                }
            }
        }
    }

    .paste-options {
        margin-top: 2rem;
    }
</style>
