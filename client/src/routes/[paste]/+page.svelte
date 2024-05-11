<script lang="ts">
    import type { PageData } from "./$types";
    import Tab from "$lib/Tab.svelte";
    import PastyMeta from "$lib/PastyMeta.svelte";
    import { deletePaste, ExpiresIn, pinPaste, starPaste, type Pasty } from "$lib/api/paste";
    import { tooltip } from "$lib/tooltips";
    import { currentUserStore } from "$lib/stores";
    import { goto } from "$app/navigation";
    import Markdown from "$lib/Markdown.svelte";

    export let data: PageData;

    let activePastyId: string = data.paste.pasties[0].id;
    let activePasty: Pasty = data.paste.pasties[0];

    let linkCopied = false;

    $: {
        const p = data.paste.pasties.find((p) => p.id === activePastyId);
        if (p) activePasty = p;
    }

    let stackedView = false;

    const setActiveTab = (id: string) => {
        activePastyId = id;
    };

    const togglePastiesView = () => {
        stackedView = !stackedView;
    };

    const onCopyLink = async () => {
        await navigator.clipboard.writeText(location.href);

        linkCopied = true;

        setTimeout(() => {
            linkCopied = false;
        }, 1000);
    };

    const onDeleteClick = async () => {
        // TODO: nicer confirm (use modal from cmd palette)
        if (confirm("are you sure you want to delete this paste? this action can't be undone.")) {
            const success = await deletePaste(data.paste.id);

            if (success) {
                goto("/");
            } else {
                // TODO: nicer error reporting.
                alert("failed to delete the paste. try again later.");
            }
        }
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
        await pinPaste(data.paste.id);

        data.paste.pinned = !data.paste.pinned;
    };
</script>

<svelte:head>
    <title>pastemyst | {data.paste.title || "untitled"}</title>
</svelte:head>

<section class="paste-header flex column center space-between">
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
                    expires {data.relativesExpiresIn}
                </span>
            {/if}
        </span>
    </div>

    <div class="options flex row center">
        {#if $currentUserStore?.id === data.paste.ownerId}
            <button
                aria-label={data.paste.pinned ? "unpin" : "pin"}
                use:tooltip={{
                    content: data.paste.pinned ? "unpin" : "pin",
                    hideOnClick: false
                }}
                on:click={onPinClick}
                class:pinned={data.paste.pinned}
            >
                {#if data.paste.pinned}
                    <svg viewBox="0 0 16 16" class="icon" width="16" height="16">
                        <title>Pin Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="m 4.456,0.734 a 1.75,1.75 0 0 1 2.826,0.504 l 0.613,1.327 a 3.081,3.081 0 0 0 2.084,1.707 l 2.454,0.584 c 1.332,0.317 1.8,1.972 0.832,2.94 L 11.06,10 l 3.72,3.72 a 0.75,0.75 0 1 1 -1.061,1.06 L 10,11.06 7.796,13.265 c -0.968,0.968 -2.623,0.5 -2.94,-0.832 L 4.272,9.979 A 3.081,3.081 0 0 0 2.565,7.895 L 1.238,7.282 A 1.75,1.75 0 0 1 0.734,4.456 Z M 5.92,1.866 A 0.25,0.25 0 0 0 5.516,1.794 L 1.794,5.516 A 0.25,0.25 0 0 0 1.866,5.92 L 3.194,6.533 A 4.582,4.582 0 0 1 5.73,9.63 l 0.584,2.454 a 0.25,0.25 0 0 0 0.42,0.12 l 5.47,-5.47 a 0.25,0.25 0 0 0 -0.12,-0.42 L 9.63,5.73 A 4.581,4.581 0 0 1 6.532,3.193 Z"
                        />
                        <path
                            fill="currentColor"
                            d="M 0.9653876,5.8199081 4.1925405,8.2195859 6.3991407,13.322349 13.322349,6.2336457 8.6609059,4.440783 5.8474906,0.66198007 Z"
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

        <button
            class="toggle-view"
            on:click={togglePastiesView}
            use:tooltip={{
                hideOnClick: false,
                content: `switch to ${stackedView ? "tabbed" : "stacked"} view`
            }}
            aria-label="switch to {stackedView ? 'tabbed' : 'stacked'} view"
        >
            {#if stackedView}
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Columns Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M2.75 0A1.75 1.75 0 001 1.75v12.5c0 .966.784 1.75 1.75 1.75h2.5A1.75 1.75 0 007 14.25V1.75A1.75 1.75 0 005.25 0h-2.5zM2.5 1.75a.25.25 0 01.25-.25h2.5a.25.25 0 01.25.25v12.5a.25.25 0 01-.25.25h-2.5a.25.25 0 01-.25-.25V1.75zM10.75 0A1.75 1.75 0 009 1.75v12.5c0 .966.784 1.75 1.75 1.75h2.5A1.75 1.75 0 0015 14.25V1.75A1.75 1.75 0 0013.25 0h-2.5zm-.25 1.75a.25.25 0 01.25-.25h2.5a.25.25 0 01.25.25v12.5a.25.25 0 01-.25.25h-2.5a.25.25 0 01-.25-.25V1.75z"
                    />
                </svg>
            {:else}
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Rows Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M16 2.75A1.75 1.75 0 0014.25 1H1.75A1.75 1.75 0 000 2.75v2.5A1.75 1.75 0 001.75 7h12.5A1.75 1.75 0 0016 5.25v-2.5zm-1.75-.25a.25.25 0 01.25.25v2.5a.25.25 0 01-.25.25H1.75a.25.25 0 01-.25-.25v-2.5a.25.25 0 01.25-.25h12.5zM16 10.75A1.75 1.75 0 0014.25 9H1.75A1.75 1.75 0 000 10.75v2.5A1.75 1.75 0 001.75 15h12.5A1.75 1.75 0 0016 13.25v-2.5zm-1.75-.25a.25.25 0 01.25.25v2.5a.25.25 0 01-.25.25H1.75a.25.25 0 01-.25-.25v-2.5a.25.25 0 01.25-.25h12.5z"
                    />
                </svg>
            {/if}
        </button>

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

        <button aria-label="more options" use:tooltip>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Dots Horizontal Icon</title>
                <path
                    fill="currentColor"
                    d="M8 9a1.5 1.5 0 100-3 1.5 1.5 0 000 3zM1.5 9a1.5 1.5 0 100-3 1.5 1.5 0 000 3zm13 0a1.5 1.5 0 100-3 1.5 1.5 0 000 3z"
                />
            </svg>
        </button>
    </div>
</section>

<div class="lang-stats flex">
    {#each data.langStats as lang}
        <div
            class="lang"
            style="width:{lang.percentage}%; background-color:{lang.language.color ??
                'var(--color-fg)'};"
            use:tooltip
            aria-label="{lang.language.name} {lang.percentage.toFixed(2)}%"
        />
    {/each}
</div>

{#if data.paste.tags}
    <div class="tags flex row center">
        {#each data.paste.tags as tag}
            <a href="/~{data.self?.username}/{tag}" class="btn">{tag}</a>
        {/each}
    </div>
{/if}

<div class="pasties">
    {#if stackedView}
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
                                />
                            </div>
                        {/if}
                    </div>
                </div>

                {#if pasty.language === "Markdown"}
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
                    />
                </div>
            {/if}
        </div>

        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        {#if activePasty.language === "Markdown"}
            <div class="markdown">
                <Markdown content={activePasty.content} />
            </div>
        {:else}
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            {@html data.highlightedCode[data.paste.pasties.findIndex((p) => p.id === activePastyId)]}
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

                span:last-child {
                    white-space: pre;
                }
            }
        }

        .options {
            button {
                margin-left: 0.5rem;

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

    .lang-stats {
        height: 5px;
        margin-bottom: 1rem;

        .lang {
            border-right: 2px solid var(--color-bg);

            &:first-child {
                border-bottom-left-radius: $border-radius;
            }

            &:last-child {
                border-bottom-right-radius: $border-radius;
                border-right: none;
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

        :global(.shiki), .markdown {
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

            .options {
                button:first-child {
                    margin-left: 0;
                }
            }
        }
    }
</style>
