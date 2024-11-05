<script lang="ts">
    import Dropdown from "./Dropdown.svelte";
    import {
        ExpiresIn,
        deletePaste,
        type PasteWithLangStats,
        pinPaste,
        togglePrivatePaste
    } from "./api/paste";
    import { cmdPalOpen, cmdPalTitle, currentUserStore } from "./stores";
    import { tooltip } from "./tooltips";
    import type { LangStat } from "./api/lang";
    import { Close, getConfirmActionCommands, setTempCommands } from "./command";
    import { invalidateAll } from "$app/navigation";
    import { formatDistanceToNow } from "date-fns";

    export let pasteWithLangStats: PasteWithLangStats;

    let dropdown: Dropdown;

    const getPasteLangs = (langStats: LangStat[]): string => {
        return langStats.map((s) => s.language.name).join(", ");
    };

    const onDelete = async () => {
        dropdown.hide();

        setTempCommands(
            getConfirmActionCommands(() => {
                (async () => {
                    const success = await deletePaste(pasteWithLangStats.paste.id);

                    if (success) {
                        invalidateAll();
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

    const onPin = async () => {
        dropdown.hide();
        await pinPaste(pasteWithLangStats.paste.id);
        invalidateAll();
    };

    const onPrivate = async () => {
        dropdown.hide();
        await togglePrivatePaste(pasteWithLangStats.paste.id);
        invalidateAll();
    };
</script>

<div class="paste">
    <div class="flex row center space-between">
        <p class="title">
            <a href="/{pasteWithLangStats.paste.id}"
                >{pasteWithLangStats.paste.title || "untitled"}</a
            >
            {#if pasteWithLangStats.paste.tags?.length > 0}
                <span>{pasteWithLangStats.paste.tags.join(", ")}</span>
            {/if}
        </p>

        <div class="flex row center gap-m">
            {#if pasteWithLangStats.paste.private}
                <div use:tooltip aria-label="private" class="flex">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Lock Closed Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M4 4v2h-.25A1.75 1.75 0 002 7.75v5.5c0 .966.784 1.75 1.75 1.75h8.5A1.75 1.75 0 0014 13.25v-5.5A1.75 1.75 0 0012.25 6H12V4a4 4 0 10-8 0zm6.5 2V4a2.5 2.5 0 00-5 0v2h5zM12 7.5h.25a.25.25 0 01.25.25v5.5a.25.25 0 01-.25.25h-8.5a.25.25 0 01-.25-.25v-5.5a.25.25 0 01.25-.25H12z"
                        />
                    </svg>
                </div>
            {:else if pasteWithLangStats.paste.pinned}
                <div use:tooltip aria-label="pinned" class="flex">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Pin Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M4.456.734a1.75 1.75 0 012.826.504l.613 1.327a3.081 3.081 0 002.084 1.707l2.454.584c1.332.317 1.8 1.972.832 2.94L11.06 10l3.72 3.72a.75.75 0 11-1.061 1.06L10 11.06l-2.204 2.205c-.968.968-2.623.5-2.94-.832l-.584-2.454a3.081 3.081 0 00-1.707-2.084l-1.327-.613a1.75 1.75 0 01-.504-2.826L4.456.734zM5.92 1.866a.25.25 0 00-.404-.072L1.794 5.516a.25.25 0 00.072.404l1.328.613A4.582 4.582 0 015.73 9.63l.584 2.454a.25.25 0 00.42.12l5.47-5.47a.25.25 0 00-.12-.42L9.63 5.73a4.581 4.581 0 01-3.098-2.537L5.92 1.866z"
                        />
                    </svg>
                </div>
            {/if}

            {#if $currentUserStore !== null}
                <Dropdown bind:this={dropdown}>
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 16 16"
                        class="icon"
                        slot="button"
                    >
                        <title>Dots Horizontal Icon</title>
                        <path
                            fill="currentColor"
                            d="M8 9a1.5 1.5 0 100-3 1.5 1.5 0 000 3zM1.5 9a1.5 1.5 0 100-3 1.5 1.5 0 000 3zm13 0a1.5 1.5 0 100-3 1.5 1.5 0 000 3z"
                        />
                    </svg>

                    <div slot="dropdown">
                        <div class="dropdown flex col gap-s">
                            <button
                                class="dropdown-option flex gap-s"
                                on:click={onPin}
                                disabled={pasteWithLangStats.paste.private}
                            >
                                {#if pasteWithLangStats.paste.pinned}
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        viewBox="0 0 16 16"
                                        class="icon"
                                    >
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
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        viewBox="0 0 16 16"
                                        class="icon"
                                    >
                                        <title>Pin Icon</title>
                                        <path
                                            fill="currentColor"
                                            fill-rule="evenodd"
                                            d="M4.456.734a1.75 1.75 0 012.826.504l.613 1.327a3.081 3.081 0 002.084 1.707l2.454.584c1.332.317 1.8 1.972.832 2.94L11.06 10l3.72 3.72a.75.75 0 11-1.061 1.06L10 11.06l-2.204 2.205c-.968.968-2.623.5-2.94-.832l-.584-2.454a3.081 3.081 0 00-1.707-2.084l-1.327-.613a1.75 1.75 0 01-.504-2.826L4.456.734zM5.92 1.866a.25.25 0 00-.404-.072L1.794 5.516a.25.25 0 00.072.404l1.328.613A4.582 4.582 0 015.73 9.63l.584 2.454a.25.25 0 00.42.12l5.47-5.47a.25.25 0 00-.12-.42L9.63 5.73a4.581 4.581 0 01-3.098-2.537L5.92 1.866z"
                                        />
                                    </svg>
                                {/if}

                                <p>{pasteWithLangStats.paste.pinned ? "unpin" : "pin"}</p>
                            </button>

                            <button
                                class="dropdown-option flex gap-s"
                                on:click={onPrivate}
                                disabled={pasteWithLangStats.paste.pinned}
                            >
                                {#if pasteWithLangStats.paste.private}
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        viewBox="0 0 16 16"
                                        class="icon"
                                    >
                                        <title>Unlock Icon</title>
                                        <path
                                            fill="currentColor"
                                            d="M5.5 4v2h7A1.5 1.5 0 0 1 14 7.5v6a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 2 13.5v-6A1.5 1.5 0 0 1 3.499 6H4V4a4 4 0 0 1 7.371-2.154.75.75 0 0 1-1.264.808A2.5 2.5 0 0 0 5.5 4Zm-2 3.5v6h9v-6h-9Z"
                                        />
                                    </svg>
                                {:else}
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        viewBox="0 0 16 16"
                                        class="icon"
                                    >
                                        <title>Lock Icon</title>
                                        <path
                                            fill="currentColor"
                                            d="M4 4a4 4 0 0 1 8 0v2h.25c.966 0 1.75.784 1.75 1.75v5.5A1.75 1.75 0 0 1 12.25 15h-8.5A1.75 1.75 0 0 1 2 13.25v-5.5C2 6.784 2.784 6 3.75 6H4Zm8.25 3.5h-8.5a.25.25 0 0 0-.25.25v5.5c0 .138.112.25.25.25h8.5a.25.25 0 0 0 .25-.25v-5.5a.25.25 0 0 0-.25-.25ZM10.5 6V4a2.5 2.5 0 1 0-5 0v2Z"
                                        />
                                    </svg>
                                {/if}

                                <p>
                                    {pasteWithLangStats.paste.private
                                        ? "set to public"
                                        : "set to private"}
                                </p>
                            </button>

                            <button class="dropdown-option delete flex gap-s" on:click={onDelete}>
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    viewBox="0 0 16 16"
                                    class="icon"
                                >
                                    <title>Trash Icon</title>
                                    <path
                                        fill="currentColor"
                                        fill-rule="evenodd"
                                        d="M6.5 1.75a.25.25 0 01.25-.25h2.5a.25.25 0 01.25.25V3h-3V1.75zm4.5 0V3h2.25a.75.75 0 010 1.5H2.75a.75.75 0 010-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75zM4.496 6.675a.75.75 0 10-1.492.15l.66 6.6A1.75 1.75 0 005.405 15h5.19c.9 0 1.652-.681 1.741-1.576l.66-6.6a.75.75 0 00-1.492-.149l-.66 6.6a.25.25 0 01-.249.225h-5.19a.25.25 0 01-.249-.225l-.66-6.6z"
                                    />
                                </svg>

                                <p>delete</p>
                            </button>
                        </div>
                    </div>
                </Dropdown>
            {/if}
        </div>
    </div>

    <div>
        <!-- prettier-ignore -->
        <span use:tooltip aria-label={new Date(pasteWithLangStats.paste.createdAt).toString()}>{formatDistanceToNow(new Date(pasteWithLangStats.paste.createdAt), { addSuffix: true })}</span>

        {#if pasteWithLangStats.paste.expiresIn !== ExpiresIn.never}
            <!-- prettier-ignore -->
            <span use:tooltip aria-label={new Date(pasteWithLangStats.paste.deletesAt).toString()}> - expires {formatDistanceToNow(new Date(pasteWithLangStats.paste.deletesAt), { addSuffix: true })}</span>
        {/if}
    </div>

    {#if pasteWithLangStats.paste.editedAt}
        <div>
            <!-- prettier-ignore -->
            <span use:tooltip aria-label={new Date(pasteWithLangStats.paste.editedAt).toString()}>edited {formatDistanceToNow(new Date(pasteWithLangStats.paste.editedAt), { addSuffix: true })}</span>
        </div>
    {/if}

    <div>
        <span>{getPasteLangs(pasteWithLangStats.languageStats)}</span>
    </div>
</div>

<style lang="scss">
    .paste {
        display: block;
        background-color: var(--color-bg);
        margin-top: 1rem;
        border-radius: $border-radius;
        padding: 0.5rem;
        font-size: $fs-medium;
        border: 1px solid var(--color-bg2);

        a {
            text-decoration: none;
        }

        p {
            margin: 0;
        }

        .icon {
            color: var(--color-bg3);
        }

        span {
            font-size: $fs-small;
            color: var(--color-bg3);
        }

        .delete,
        .delete .icon {
            color: var(--color-danger);
        }

        .dropdown-option {
            border-color: var(--color-bg1);
            background-color: var(--color-bg1);

            &:hover {
                background-color: var(--color-bg2);
                border-color: var(--color-bg3);
            }

            &:focus,
            &:active {
                border-color: var(--color-primary);
            }
        }
    }
</style>
