<script lang="ts">
    import type { PageData } from "./$types";
    import Tab from "$lib/Tab.svelte";
    import PastyMeta from "$lib/PastyMeta.svelte";
    import {
        deletePaste,
        ExpiresIn,
        pinPaste,
        starPaste,
        type Pasty,
        togglePrivatePaste
    } from "$lib/api/paste";
    import { tooltip } from "$lib/tooltips";
    import {
        cmdPalOpen,
        cmdPalTitle,
        copyLinkToClipboardStore,
        currentUserStore
    } from "$lib/stores";
    import { goto } from "$app/navigation";
    import Markdown from "$lib/Markdown.svelte";
    import { isLanguageMarkdown } from "$lib/utils/markdown";
    import { setTempCommands, getConfirmActionCommands, Close } from "$lib/command";
    import { onMount } from "svelte";
    import toast from "svelte-french-toast";
    import { env } from "$env/dynamic/public";
    import { humanFileSize } from "$lib/strings";

    export let data: PageData;

    let activePastyId: string = data.paste.pasties[0].id;
    let activePasty: Pasty = data.paste.pasties[0];

    let previewMarkdownStacked: boolean[] = [];
    let previewMarkdownTabbed: boolean;

    let linkCopied = false;

    $: {
        const p = data.paste.pasties.find((p) => p.id === activePastyId);
        if (p) activePasty = p;
    }

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

    const setActiveTab = (id: string) => {
        activePastyId = id;
    };

    const onCopyLink = async () => {
        await navigator.clipboard.writeText(location.href);

        linkCopied = true;

        setTimeout(() => {
            linkCopied = false;
        }, 1000);
    };

    const onDeleteClick = async () => {
        setTempCommands(
            getConfirmActionCommands(() => {
                (async () => {
                    const success = await deletePaste(data.paste.id);

                    if (success) {
                        goto("/");
                    } else {
                        // TODO: nicer error reporting.
                        alert("failed to delete the paste. try again later.");
                    }
                })();

                return Close.yes;
            })
        );

        cmdPalTitle.set("are you sure you want to delete this paste? this action can't be undone.");
        cmdPalOpen.set(true);
    };

    const onStarClick = async () => {
        await starPaste(data.paste.id);

        data.isStarred = !data.isStarred;

        if (data.isStarred) {
            data.paste.stars++;
        } else {
            data.paste.stars--;
        }
    };

    const onPinClick = async () => {
        const ok = await pinPaste(data.paste.id);

        if (ok) {
            data.paste.pinned = !data.paste.pinned;
        }
    };

    const onPrivateClick = async () => {
        const ok = await togglePrivatePaste(data.paste.id);

        if (ok) {
            data.paste.private = !data.paste.private;
        }
    };

    const colorIsDark = (bgColor: string): boolean => {
        let color = bgColor.charAt(0) === "#" ? bgColor.substring(1, 7) : bgColor;
        let r = parseInt(color.substring(0, 2), 16);
        let g = parseInt(color.substring(2, 4), 16);
        let b = parseInt(color.substring(4, 6), 16);
        let uicolors = [r / 255, g / 255, b / 255];
        let c = uicolors.map((col) => {
            if (col <= 0.03928) {
                return col / 12.92;
            }
            return Math.pow((col + 0.055) / 1.055, 2.4);
        });
        let L = 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];
        return L <= 0.179;
    };
</script>

<svelte:head>
    <title>pastemyst | {data.paste.title || "untitled"}</title>
</svelte:head>

<section class="paste-header flex row center space-between">
    <div class="title flex col">
        <div class="flex row center">
            {#if data.paste.private}
                <div use:tooltip aria-label="private paste" class="flex row center private-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Lock Closed Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M4 4v2h-.25A1.75 1.75 0 002 7.75v5.5c0 .966.784 1.75 1.75 1.75h8.5A1.75 1.75 0 0014 13.25v-5.5A1.75 1.75 0 0012.25 6H12V4a4 4 0 10-8 0zm6.5 2V4a2.5 2.5 0 00-5 0v2h5zM12 7.5h.25a.25.25 0 01.25.25v5.5a.25.25 0 01-.25.25h-8.5a.25.25 0 01-.25-.25v-5.5a.25.25 0 01.25-.25H12z"
                        />
                    </svg>
                </div>
            {/if}

            <h2>{data.paste.title || "untitled"}</h2>
        </div>

        <span class="dates">
            <span use:tooltip aria-label={new Date(data.paste.createdAt).toString()}>
                {data.relativeCreatedAt}
            </span>
            {#if data.owner}
                <span class="owner"
                    >by <a href="/~{data.owner.username}">{data.owner.username}</a></span
                >
            {/if}
            {#if data.paste.expiresIn != ExpiresIn.never}
                <span class="separator">-</span>
                <span use:tooltip aria-label={new Date(data.paste.deletesAt).toString()}>
                    expires {data.relativeExpiresIn}
                </span>
            {/if}
            {#if data.relativeEditedAt && data.paste.editedAt}
                <span use:tooltip aria-label={new Date(data.paste.editedAt).toString()}>
                    <br />edited {data.relativeEditedAt}
                </span>
            {/if}
        </span>
    </div>

    <div class="options flex wrap row center gap-s">
        {#if $currentUserStore?.id === data.paste.ownerId}
            <button
                aria-label={data.paste.private
                    ? "can't pin a private paste"
                    : data.paste.pinned
                      ? "unpin"
                      : "pin"}
                use:tooltip={{
                    content: data.paste.private
                        ? "can't pin a private paste"
                        : data.paste.pinned
                          ? "unpin"
                          : "pin",
                    hideOnClick: false
                }}
                on:click={onPinClick}
                class:pinned={data.paste.pinned}
                class:disabled={data.paste.private}
            >
                {#if data.paste.pinned}
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Unpin Icon</title>
                        <path
                            fill="currentColor"
                            d="m1.655.595 13.75 13.75q.22.219.22.53 0 .311-.22.53-.219.22-.53.22-.311 0-.53-.22L.595 1.655q-.22-.219-.22-.53 0-.311.22-.53.219-.22.53-.22.311 0 .53.22ZM.72 14.22l4.5-4.5q.219-.22.53-.22.311 0 .53.22.22.219.22.53 0 .311-.22.53l-4.5 4.5q-.219.22-.53.22-.311 0-.53-.22-.22-.219-.22-.53 0-.311.22-.53Z"
                        />
                        <path
                            fill="currentColor"
                            d="m5.424 6.146-1.759.419q-.143.034-.183.175-.04.141.064.245l5.469 5.469q.104.104.245.064.141-.04.175-.183l.359-1.509q.072-.302.337-.465.264-.163.567-.091.302.072.465.337.162.264.09.567l-.359 1.509q-.238.999-1.226 1.278-.988.28-1.714-.446L2.485 8.046q-.726-.726-.446-1.714.279-.988 1.278-1.226l1.759-.419q.303-.072.567.091.265.163.337.465.072.302-.091.567-.163.264-.465.336ZM7.47 3.47q.155-.156.247-.355l.751-1.627Q8.851.659 9.75.498q.899-.16 1.544.486l3.722 3.722q.646.645.486 1.544-.161.899-.99 1.282l-1.627.751q-.199.092-.355.247-.219.22-.53.22-.311 0-.53-.22-.22-.219-.22-.53 0-.311.22-.53.344-.345.787-.549l1.627-.751q.118-.055.141-.183.023-.128-.069-.221l-3.722-3.722q-.092-.092-.221-.069-.128.023-.183.141l-.751 1.627q-.204.443-.549.787-.219.22-.53.22-.311 0-.53-.22-.22-.219-.22-.53 0-.311.22-.53Z"
                        />
                    </svg>
                {:else}
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Pin Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M4.456.734a1.75 1.75 0 012.826.504l.613 1.327a3.081 3.081 0 002.084 1.707l2.454.584c1.332.317 1.8 1.972.832 2.94L11.06 10l3.72 3.72a.75.75 0 11-1.061 1.06L10 11.06l-2.204 2.205c-.968.968-2.623.5-2.94-.832l-.584-2.454a3.081 3.081 0 00-1.707-2.084l-1.327-.613a1.75 1.75 0 01-.504-2.826L4.456.734zM5.92 1.866a.25.25 0 00-.404-.072L1.794 5.516a.25.25 0 00.072.404l1.328.613A4.582 4.582 0 015.73 9.63l.584 2.454a.25.25 0 00.42.12l5.47-5.47a.25.25 0 00-.12-.42L9.63 5.73a4.581 4.581 0 01-3.098-2.537L5.92 1.866z"
                        />
                    </svg>
                {/if}
            </button>
        {/if}

        <button
            class="stars"
            aria-label="stars"
            use:tooltip
            disabled={$currentUserStore == null}
            on:click={onStarClick}
            class:starred={data.isStarred}
        >
            {#if data.isStarred}
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Star Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M8 .25a.75.75 0 01.673.418l1.882 3.815 4.21.612a.75.75 0 01.416 1.279l-3.046 2.97.719 4.192a.75.75 0 01-1.088.791L8 12.347l-3.766 1.98a.75.75 0 01-1.088-.79l.72-4.194L.818 6.374a.75.75 0 01.416-1.28l4.21-.611L7.327.668A.75.75 0 018 .25z"
                    />
                </svg>
            {:else}
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Star Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M8 .25a.75.75 0 01.673.418l1.882 3.815 4.21.612a.75.75 0 01.416 1.279l-3.046 2.97.719 4.192a.75.75 0 01-1.088.791L8 12.347l-3.766 1.98a.75.75 0 01-1.088-.79l.72-4.194L.818 6.374a.75.75 0 01.416-1.28l4.21-.611L7.327.668A.75.75 0 018 .25zm0 2.445L6.615 5.5a.75.75 0 01-.564.41l-3.097.45 2.24 2.184a.75.75 0 01.216.664l-.528 3.084 2.769-1.456a.75.75 0 01.698 0l2.77 1.456-.53-3.084a.75.75 0 01.216-.664l2.24-2.183-3.096-.45a.75.75 0 01-.564-.41L8 2.694v.001z"
                    />
                </svg>
            {/if}
            <p>{data.paste.stars}</p>
        </button>

        {#if data.paste.private && $currentUserStore?.id === data.paste.ownerId}
            <button aria-label="set to public" use:tooltip on:click={onPrivateClick}>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Unlock Icon</title>
                    <path
                        fill="currentColor"
                        d="M5.5 4v2h7A1.5 1.5 0 0 1 14 7.5v6a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 2 13.5v-6A1.5 1.5 0 0 1 3.499 6H4V4a4 4 0 0 1 7.371-2.154.75.75 0 0 1-1.264.808A2.5 2.5 0 0 0 5.5 4Zm-2 3.5v6h9v-6h-9Z"
                    />
                </svg>
            </button>
        {:else}
            <button
                aria-label={data.paste.pinned ? "can't private a pinned paste" : "set to private"}
                use:tooltip={{
                    content: data.paste.pinned ? "can't private a pinned paste" : "set to private",
                    hideOnClick: false
                }}
                on:click={onPrivateClick}
                class:disabled={data.paste.pinned}
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Lock Icon</title>
                    <path
                        fill="currentColor"
                        d="M4 4a4 4 0 0 1 8 0v2h.25c.966 0 1.75.784 1.75 1.75v5.5A1.75 1.75 0 0 1 12.25 15h-8.5A1.75 1.75 0 0 1 2 13.25v-5.5C2 6.784 2.784 6 3.75 6H4Zm8.25 3.5h-8.5a.25.25 0 0 0-.25.25v5.5c0 .138.112.25.25.25h8.5a.25.25 0 0 0 .25-.25v-5.5a.25.25 0 0 0-.25-.25ZM10.5 6V4a2.5 2.5 0 1 0-5 0v2Z"
                    />
                </svg>
            </button>
        {/if}

        <button aria-label="edit" use:tooltip>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Pen Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M11.013 1.427a1.75 1.75 0 012.474 0l1.086 1.086a1.75 1.75 0 010 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 01-.927-.928l.929-3.25a1.75 1.75 0 01.445-.758l8.61-8.61zm1.414 1.06a.25.25 0 00-.354 0L10.811 3.75l1.439 1.44 1.263-1.263a.25.25 0 000-.354l-1.086-1.086zM11.189 6.25L9.75 4.81l-6.286 6.287a.25.25 0 00-.064.108l-.558 1.953 1.953-.558a.249.249 0 00.108-.064l6.286-6.286z"
                />
            </svg>
        </button>

        <button
            aria-label="copy link"
            use:tooltip={{
                content: linkCopied ? "copied" : "copy link",
                hideOnClick: false
            }}
            on:click={onCopyLink}
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Link Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M7.775 3.275a.75.75 0 001.06 1.06l1.25-1.25a2 2 0 112.83 2.83l-2.5 2.5a2 2 0 01-2.83 0 .75.75 0 00-1.06 1.06 3.5 3.5 0 004.95 0l2.5-2.5a3.5 3.5 0 00-4.95-4.95l-1.25 1.25zm-4.69 9.64a2 2 0 010-2.83l2.5-2.5a2 2 0 012.83 0 .75.75 0 001.06-1.06 3.5 3.5 0 00-4.95 0l-2.5 2.5a3.5 3.5 0 004.95 4.95l1.25-1.25a.75.75 0 00-1.06-1.06l-1.25 1.25a2 2 0 01-2.83 0z"
                />
            </svg>
        </button>

        <button aria-label="clone paste" use:tooltip>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Duplicate Icon</title>
                <path
                    fill="currentColor"
                    d="M10.5 3a.75.75 0 01.75.75v1h1a.75.75 0 010 1.5h-1v1a.75.75 0 01-1.5 0v-1h-1a.75.75 0 010-1.5h1v-1A.75.75 0 0110.5 3z"
                />
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M6.75 0A1.75 1.75 0 005 1.75v7.5c0 .966.784 1.75 1.75 1.75h7.5A1.75 1.75 0 0016 9.25v-7.5A1.75 1.75 0 0014.25 0h-7.5zM6.5 1.75a.25.25 0 01.25-.25h7.5a.25.25 0 01.25.25v7.5a.25.25 0 01-.25.25h-7.5a.25.25 0 01-.25-.25v-7.5z"
                />
                <path
                    fill="currentColor"
                    d="M1.75 5A1.75 1.75 0 000 6.75v7.5C0 15.216.784 16 1.75 16h7.5A1.75 1.75 0 0011 14.25v-1.5a.75.75 0 00-1.5 0v1.5a.25.25 0 01-.25.25h-7.5a.25.25 0 01-.25-.25v-7.5a.25.25 0 01.25-.25h1.5a.75.75 0 000-1.5h-1.5z"
                />
            </svg>
        </button>

        <a
            href="{env.PUBLIC_API_BASE}/pastes/{data.paste.id}.zip"
            class="btn"
            aria-label="download paste"
            use:tooltip
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Download Icon</title>
                <path
                    fill="currentColor"
                    d="M2.75 14A1.75 1.75 0 0 1 1 12.25v-2.5a.75.75 0 0 1 1.5 0v2.5c0 .138.112.25.25.25h10.5a.25.25 0 0 0 .25-.25v-2.5a.75.75 0 0 1 1.5 0v2.5A1.75 1.75 0 0 1 13.25 14Z"
                />
                <path
                    fill="currentColor"
                    d="M7.25 7.689V2a.75.75 0 0 1 1.5 0v5.689l1.97-1.969a.749.749 0 1 1 1.06 1.06l-3.25 3.25a.749.749 0 0 1-1.06 0L4.22 6.78a.749.749 0 1 1 1.06-1.06l1.97 1.969Z"
                />
            </svg>
        </a>

        {#if data.paste.ownerId === $currentUserStore?.id}
            <button
                use:tooltip
                aria-label="delete paste"
                on:click={onDeleteClick}
                class="btn-danger"
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Trash Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M6.5 1.75a.25.25 0 01.25-.25h2.5a.25.25 0 01.25.25V3h-3V1.75zm4.5 0V3h2.25a.75.75 0 010 1.5H2.75a.75.75 0 010-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75zM4.496 6.675a.75.75 0 10-1.492.15l.66 6.6A1.75 1.75 0 005.405 15h5.19c.9 0 1.652-.681 1.741-1.576l.66-6.6a.75.75 0 00-1.492-.149l-.66 6.6a.25.25 0 01-.249.225h-5.19a.25.25 0 01-.249-.225l-.66-6.6z"
                    />
                </svg>
            </button>
        {/if}
    </div>
</section>

{#if data.pasteStats}
    <div class="paste-stats flex row center space-between">
        <div class="size-stats">
            <span>
                {data.pasteStats.lines} line{data.pasteStats.lines > 1 ? "s" : ""}
            </span>

            <span>
                {data.pasteStats.words} word{data.pasteStats.words > 1 ? "s" : ""}
            </span>

            <span>
                {humanFileSize(data.pasteStats.bytes).toLowerCase()}
            </span>
        </div>

        <div class="lang-stats flex wrap row center gap-m">
            {#each data.langStats as lang}
                <div
                    class="lang flex row center gap-s"
                    style="background-color: {lang.language.color ?? 'var(--color-fg)'}"
                    class:dark={colorIsDark(lang.language.color ?? "#ffffff")}
                >
                    <span>{lang.language.name}</span>
                    <span>{lang.percentage.toFixed(2)}%</span>
                </div>
            {/each}
        </div>
    </div>
{/if}

{#if data.paste.tags}
    <div class="tags flex row center">
        {#each data.paste.tags as tag}
            <a href="/~{data.self?.username}?tag={tag}" class="btn">{tag}</a>
        {/each}
    </div>
{/if}

<div class="pasties">
    {#if data.settings.pasteView === "stacked"}
        {#each data.paste.pasties as pasty, i}
            <div class="pasty">
                <div class="sticky">
                    <div class="title flex row space-between center">
                        <span>{pasty.title || "untitled"}</span>

                        {#if data.pasteStats}
                            <div class="meta-stacked flex row center">
                                <PastyMeta
                                    paste={data.paste}
                                    {pasty}
                                    langStats={data.langStats}
                                    stats={data.pasteStats.pasties[pasty.id]}
                                    bind:previewMarkdown={previewMarkdownStacked[i]}
                                />
                            </div>
                        {/if}
                    </div>
                </div>

                {#if isLanguageMarkdown(pasty.language) && previewMarkdownStacked[i]}
                    <div class="markdown">
                        <Markdown content={pasty.content} />
                    </div>
                {:else}
                    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                    {@html data.highlightedCode[i]}
                {/if}
            </div>
        {/each}
    {:else}
        <div class="sticky">
            <div class="tabs flex row center">
                <div class="tabgroup flex row">
                    {#each data.paste.pasties as pasty}
                        <Tab
                            id={pasty.id}
                            isReadonly
                            title={pasty.title}
                            isActive={pasty.id === activePastyId}
                            on:click={() => setActiveTab(pasty.id)}
                        />
                    {/each}
                </div>
            </div>

            {#if data.pasteStats}
                <div class="meta-tabbed">
                    <PastyMeta
                        paste={data.paste}
                        pasty={activePasty}
                        langStats={data.langStats}
                        stats={data.pasteStats.pasties[activePastyId]}
                        bind:previewMarkdown={previewMarkdownTabbed}
                    />
                </div>
            {/if}
        </div>

        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        {#if isLanguageMarkdown(activePasty.language) && previewMarkdownTabbed}
            <div class="markdown">
                <Markdown content={activePasty.content} />
            </div>
        {:else}
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            {@html data.highlightedCode[
                data.paste.pasties.findIndex((p) => p.id === activePastyId)
            ]}
        {/if}
    {/if}
</div>

<style lang="scss">
    .paste-header {
        padding: 0.5rem 1rem;
        margin-bottom: 0;
        border-bottom-left-radius: 0;
        border-bottom-right-radius: 0;
        border-bottom: none;

        .title {
            margin: 0;

            .private-icon {
                max-width: 18px;
                margin-right: 0.5rem;
            }

            h2 {
                font-weight: normal;
                font-size: $fs-medium;
                margin: 0;
                margin-right: 1.5rem;
                word-break: break-word;
            }

            .dates {
                font-size: $fs-small;
                color: var(--color-bg3);
                margin-top: 0.25rem;
                line-height: 1.5;

                span:last-child {
                    white-space: pre;
                }
            }
        }

        .options {
            button,
            .btn {
                svg {
                    max-width: 20px;
                    max-height: 20px;
                }

                &.starred svg,
                &.pinned svg {
                    color: var(--color-primary);
                }
            }

            .stars {
                p {
                    margin: 0;
                    margin-left: 0.5rem;
                    font-size: $fs-small;
                }
            }
        }
    }

    .paste-stats {
        font-size: $fs-small;
        border: 1px solid var(--color-bg2);
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
        color: var(--color-bg3);
        background-color: var(--color-bg1);
        padding: 0.25rem 1rem;

        .size-stats {
            span::after {
                content: "|";
                opacity: 0.3;
                font-size: $fs-medium;
                margin: 0 0.25rem;
            }

            span:last-child::after {
                content: "";
            }
        }

        .lang-stats {
            .lang {
                border-radius: $border-radius;
                padding: 0.25rem 0.5rem;
                color: var(--color-bg);

                &.dark {
                    color: var(--color-fg);
                }
            }
        }
    }

    .tags {
        font-size: $fs-small;
        gap: 0.5rem;
        flex-wrap: wrap;
    }

    .pasties {
        margin-top: 1rem;

        .tabs {
            width: 100%;
            box-sizing: border-box;
            background-color: var(--color-bg2);
            border-radius: $border-radius $border-radius 0 0;
            border-bottom: 1px solid var(--color-bg2);

            .tabgroup {
                flex-wrap: wrap;
                width: 100%;

                :global(.tab) {
                    flex-grow: 0;
                }
            }
        }

        .meta-tabbed {
            background-color: var(--color-bg1);
            padding: 0.25rem;
            border: 1px solid var(--color-bg2);
            border-top: none;
        }

        .pasty {
            margin-bottom: 2rem;

            &:last-child {
                margin-bottom: 0;
            }

            .title {
                background-color: var(--color-bg1);
                border-radius: $border-radius $border-radius 0 0;
                border-bottom: 1px solid var(--color-primary);
                width: 100%;
                box-sizing: border-box;

                span {
                    padding: 0.5rem 1rem;
                }

                .meta-stacked {
                    padding-right: 1rem;
                }
            }
        }

        .markdown {
            padding: 0rem 1rem;
        }

        :global(.shiki),
        .markdown {
            border-bottom-left-radius: $border-radius;
            border-bottom-right-radius: $border-radius;
            border: 1px solid var(--color-bg2);
            border-top: none;
            margin: 0;
            overflow-x: auto;
        }

        :global(.shiki code) {
            border: none;
            font-size: $fs-normal;
            padding: 0;
            border-radius: 0;
            background-color: transparent;
        }

        :global(.shiki code) {
            counter-reset: step;
            counter-increment: step 0;
        }

        :global(.shiki code .line::before) {
            content: counter(step);
            counter-increment: step;
            width: 1rem;
            margin-right: 1rem;
            display: inline-block;
            text-align: right;
            color: var(--color-bg3);
            font-size: $fs-normal;
        }
    }

    .sticky {
        position: sticky;
        top: 0;
        z-index: 10;
    }

    @media screen and (max-width: 620px) {
        .title {
            .dates {
                display: flex;
                flex-direction: column;
            }

            .separator {
                display: none;
            }
        }
    }

    @media screen and (max-width: 720px) {
        .paste-header {
            flex-direction: column;
            align-items: baseline;

            .title {
                margin-bottom: 1rem;
            }
        }

        .paste-stats {
            flex-direction: column;
            align-items: baseline;

            .size-stats {
                margin-bottom: 1rem;
            }

            .lang-stats {
                margin-bottom: 1rem;
            }
        }
    }
</style>
