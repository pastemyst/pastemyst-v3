<script lang="ts">
    import { goto } from "$app/navigation";
    import {
        createPaste,
        ExpiresIn,
        expiresInToLongString,
        type PasteCreateInfo,
        type PastyCreateInfo
    } from "$lib/api/paste";
    import { addBaseCommands, Close, setTempCommands, type Command } from "$lib/command";
    import PasteOptions from "$lib/PasteOptions.svelte";
    import {
        cmdPalOpen,
        cmdPalTitle,
        copyLinkToClipboardStore,
        creatingPasteStore,
        currentUserStore
    } from "$lib/stores.svelte";
    import TabbedEditor from "$lib/TabbedEditor.svelte";
    import TagInput from "$lib/TagInput.svelte";
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import { addToast } from "$lib/toasts.svelte";

    interface Props {
        data: PageData;
        selectedExpiresIn?: ExpiresIn;
    }

    let { data, selectedExpiresIn = $bindable(ExpiresIn.never) }: Props = $props();

    let title: string = $state("");

    let tags: string[] = $state([]);

    let anonymous: boolean = $state(false);
    let isPrivate: boolean = $state(false);
    let pinned: boolean = $state(false);
    let encrypt: boolean = $state(false);
    let encryptionKey: string = $state("");
    let creatingPaste: boolean = $state(false);

    let editor: TabbedEditor;

    $effect(() => {
        if (anonymous) tags = [];
    });

    onMount(() => {
        const commands: Command[] = [
            {
                name: "set expires in",
                action: () => {
                    setTempCommands(getExpiresInCommands());
                    return Close.no;
                }
            },
            {
                name: "set editor language",
                action: () => {
                    const cmds = editor.getLanguageCommands();
                    (async () => {
                        if (cmds) setTempCommands(await cmds);
                    })();
                    return Close.no;
                }
            },
            {
                name: "set editor indentation",
                action: () => {
                    const cmds = editor.getIndentUnitCommands();
                    if (cmds) setTempCommands(cmds);
                    return Close.no;
                }
            },
            {
                name: "convert indentation",
                action: () => {
                    const cmds = editor.getIndentUnitCommands(true);
                    if (cmds) setTempCommands(cmds);
                    return Close.no;
                }
            }
        ];

        addBaseCommands(commands);
    });

    const onCreatePaste = async () => {
        creatingPaste = true;

        let pasties: PastyCreateInfo[] = [];

        for (const tab of editor.getTabs()) {
            pasties.push({
                title: tab.title!,
                content: tab.content,
                language: tab.language?.name ?? "Text"
            });
        }

        const pasteSkeleton: PasteCreateInfo = {
            title: title,
            expiresIn: selectedExpiresIn,
            pasties: pasties,
            anonymous: anonymous,
            private: isPrivate,
            pinned: pinned,
            encrypted: encrypt,
            tags: tags
        };

        const [paste, createPasteError] = await createPaste(pasteSkeleton, encryptionKey);

        if (paste) {
            $creatingPasteStore = true;

            if (data.settings.copyLinkOnCreate) {
                await navigator.clipboard.writeText(`${window.location}${paste?.id}`);
                copyLinkToClipboardStore.set(true);
            }

            await goto(`/${paste.id}`);
        } else {
            addToast(`failed to create paste: ${createPasteError!.message.toLowerCase()}`, "error");

            creatingPaste = false;
        }
    };

    const getExpiresInCommands = (): Command[] => {
        const commands: Command[] = [];

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        for (const [_, exp] of Object.entries(ExpiresIn)) {
            commands.push({
                name: expiresInToLongString(exp),
                description: exp.toString(),
                action: () => {
                    selectedExpiresIn = exp;
                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const openExpiresSelect = () => {
        setTempCommands(getExpiresInCommands());

        cmdPalTitle.set("select when the paste will expire");
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst</title>
    <meta property="og:title" content="pastemyst" />
    <meta property="twitter:title" content="pastemyst" />
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
        bind:value={title}
    />

    <button onclick={openExpiresSelect}>
        expires in: {expiresInToLongString(selectedExpiresIn)}
    </button>
</div>

{#if $currentUserStore}
    <TagInput bind:tags existingTags={data.userTags} anonymousPaste={anonymous} readonly={false} />
{/if}

<TabbedEditor settings={data.settings} bind:this={editor} />

<div class="paste-options">
    <PasteOptions
        oncreatePaste={onCreatePaste}
        bind:anonymous
        bind:isPrivate
        bind:pinned
        bind:encrypt
        bind:encryptionKey
        bind:loading={creatingPaste}
    />
</div>

<style lang="scss">
    .title-input {
        margin-top: 2rem;
        margin-bottom: 1rem;

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
            border-left-color: var(--color-bg1);

            &:hover {
                border-left-color: var(--color-bg3);
            }

            &:focus,
            &:active {
                border-left-color: var(--color-primary);
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
                border-left-color: var(--color-bg2);
                border-top-color: var(--color-bg1);

                &:hover {
                    border-top-color: var(--color-bg3);
                }

                &:focus,
                &:active {
                    border-top-color: var(--color-primary);
                }
            }
        }
    }

    .paste-options {
        margin-top: 2rem;
    }
</style>
