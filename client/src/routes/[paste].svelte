<script lang="ts" context="module">
    import {
        ExpiresIn,
        getPaste,
        getPasteLangs,
        getPasteStats,
        type Paste,
        type PasteStats,
        type Pasty
    } from "$lib/api/paste";
    import { tooltip } from "$lib/tooltips";
    import moment from "moment";

    export const load: Load = async ({ params, fetch }) => {
        const [paste, pasteStatus] = await getPaste(fetch, params.paste);

        if (!paste) {
            // TODO: error handling
            return {
                status: pasteStatus
            };
        }

        const relativeCreatedAt = moment(paste.createdAt).fromNow();
        const relativesExpiresIn =
            paste.expiresIn !== ExpiresIn.never ? moment(paste.deletesAt).fromNow() : "";
        const pasteStats = await getPasteStats(fetch, paste.id);
        const langStats = await getPasteLangs(fetch, paste.id);
        const [owner, ownerStatus] =
            paste.ownerId !== "" ? await getUserById(fetch, paste.ownerId) : [null, 0];

        if (paste.ownerId !== "" && !owner) {
            // TODO: error handling
            return {
                status: ownerStatus
            };
        }

        const highlightedCode: string[] = [];

        for (const pasty of paste.pasties) {
            const res = await fetch("/internal/highlight", {
                method: "post",
                body: JSON.stringify({
                    content: pasty.content,
                    language: pasty.language
                })
            });

            // TODO: error handling
            if (!res.ok)
                return {
                    status: 500
                };

            highlightedCode.push(await res.text());
        }

        return {
            status: 200,
            props: {
                paste: paste,
                relativeCreatedAt: relativeCreatedAt,
                relativesExpiresIn: relativesExpiresIn,
                langStats: langStats,
                pasteStats: pasteStats,
                owner: owner,
                highlightedCode: highlightedCode
            }
        };
    };
</script>

<script lang="ts">
    import Tab from "$lib/Tab.svelte";
    import { getUserById, type User } from "$lib/api/user";
    import type { LangStat } from "$lib/api/lang";
    import PastyMeta from "$lib/PastyMeta.svelte";
    import type { Load } from "@sveltejs/kit";

    export let paste: Paste;
    export let relativeCreatedAt: string;
    export let relativesExpiresIn: string;
    export let highlightedCode: string[];
    export let owner: User | null;
    export let langStats: LangStat[];
    export let pasteStats: PasteStats;

    let activePastyId: string = paste.pasties[0].id;
    let activePasty: Pasty = paste.pasties[0];

    $: {
        const p = paste.pasties.find((p) => p.id === activePastyId);
        if (p) activePasty = p;
    }

    let stackedView = false;

    const setActiveTab = (id: string) => {
        activePastyId = id;
    };

    const togglePastiesView = () => {
        stackedView = !stackedView;
    };
</script>

<svelte:head>
    <title>pastemyst | {paste.title || "untitled"}</title>
</svelte:head>

<section class="paste-header flex column center space-between">
    <div class="title flex col">
        <div class="flex row center">
            {#if paste.private}
                <div use:tooltip aria-label="private paste" class="flex row center private-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                        <title>Lock Closed</title>
                        <path
                            fill="currentColor"
                            d="M368 192h-16v-80a96 96 0 10-192 0v80h-16a64.07 64.07 0 00-64 64v176a64.07 64.07 0 0064 64h224a64.07 64.07 0 0064-64V256a64.07 64.07 0 00-64-64zm-48 0H192v-80a64 64 0 11128 0z"
                        />
                    </svg>
                </div>
            {/if}

            <h2>{paste.title || "untitled"}</h2>
        </div>

        <span class="dates">
            <span use:tooltip aria-label={new Date(paste.createdAt).toString()}>
                {relativeCreatedAt}
            </span>
            {#if owner}
                <span class="owner">by <a href="/~{owner.username}">{owner.username}</a></span>
            {/if}
            {#if paste?.expiresIn != ExpiresIn.never}
                <span class="separator">-</span>
                <span use:tooltip aria-label={new Date(paste.deletesAt).toString()}>
                    expires {relativesExpiresIn}
                </span>
            {/if}
        </span>
    </div>

    <div class="options flex row center">
        <div class="btn stars" aria-label="stars" use:tooltip>
            <svg
                class="icon"
                xmlns="http://www.w3.org/2000/svg"
                width="512"
                height="512"
                viewBox="0 0 512 512"
            >
                <title>star</title>
                <path
                    fill="currentColor"
                    d="M394,480a16,16,0,0,1-9.39-3L256,383.76,127.39,477a16,16,0,0,1-24.55-18.08L153,310.35,23,221.2A16,16,0,0,1,32,192H192.38l48.4-148.95a16,16,0,0,1,30.44,0l48.4,149H480a16,16,0,0,1,9.05,29.2L359,310.35l50.13,148.53A16,16,0,0,1,394,480Z"
                />
            </svg>
            <p>54</p>
        </div>

        <div class="btn" aria-label="edit" use:tooltip>
            <svg
                class="icon"
                xmlns="http://www.w3.org/2000/svg"
                width="512"
                height="512"
                viewBox="0 0 512 512"
            >
                <title>edit</title>
                <path
                    fill="currentColor"
                    d="M459.94,53.25a16.06,16.06,0,0,0-23.22-.56L424.35,65a8,8,0,0,0,0,11.31l11.34,11.32a8,8,0,0,0,11.34,0l12.06-12C465.19,69.54,465.76,59.62,459.94,53.25Z"
                />
                <path
                    fill="currentColor"
                    d="M399.34,90,218.82,270.2a9,9,0,0,0-2.31,3.93L208.16,299a3.91,3.91,0,0,0,4.86,4.86l24.85-8.35a9,9,0,0,0,3.93-2.31L422,112.66A9,9,0,0,0,422,100L412.05,90A9,9,0,0,0,399.34,90Z"
                />
                <path
                    fill="currentColor"
                    d="M386.34,193.66,264.45,315.79A41.08,41.08,0,0,1,247.58,326l-25.9,8.67a35.92,35.92,0,0,1-44.33-44.33l8.67-25.9a41.08,41.08,0,0,1,10.19-16.87L318.34,125.66A8,8,0,0,0,312.69,112H104a56,56,0,0,0-56,56V408a56,56,0,0,0,56,56H344a56,56,0,0,0,56-56V199.31A8,8,0,0,0,386.34,193.66Z"
                />
            </svg>
        </div>

        <div class="btn" aria-label="copy link" use:tooltip>
            <svg
                class="icon"
                xmlns="http://www.w3.org/2000/svg"
                width="512"
                height="512"
                viewBox="0 0 512 512"
            >
                <title>copy link</title>
                <path
                    stroke="currentColor"
                    d="M200.66,352H144a96,96,0,0,1,0-192h55.41"
                    style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"
                />
                <path
                    stroke="currentColor"
                    d="M312.59,160H368a96,96,0,0,1,0,192H311.34"
                    style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"
                />
                <line
                    stroke="currentColor"
                    x1="169.07"
                    y1="256"
                    x2="344.93"
                    y2="256"
                    style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"
                />
            </svg>
        </div>

        <div class="btn" aria-label="clone paste" use:tooltip>
            <svg
                class="icon"
                xmlns="http://www.w3.org/2000/svg"
                width="512"
                height="512"
                viewBox="0 0 512 512"
            >
                <title>clone paste</title>
                <path
                    fill="currentColor"
                    d="M408,112H184a72,72,0,0,0-72,72V408a72,72,0,0,0,72,72H408a72,72,0,0,0,72-72V184A72,72,0,0,0,408,112ZM375.55,312H312v63.55c0,8.61-6.62,16-15.23,16.43A16,16,0,0,1,280,376V312H216.45c-8.61,0-16-6.62-16.43-15.23A16,16,0,0,1,216,280h64V216.45c0-8.61,6.62-16,15.23-16.43A16,16,0,0,1,312,216v64h64a16,16,0,0,1,16,16.77C391.58,305.38,384.16,312,375.55,312Z"
                />
                <path
                    fill="currentColor"
                    d="M395.88,80A72.12,72.12,0,0,0,328,32H104a72,72,0,0,0-72,72V328a72.12,72.12,0,0,0,48,67.88V160a80,80,0,0,1,80-80Z"
                />
            </svg>
        </div>

        <button
            class="toggle-view btn"
            on:click={togglePastiesView}
            use:tooltip
            aria-label="toggle stacked / tabbed view"
        >
            <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                <title>Layers</title>
                <path
                    fill="currentColor"
                    d="M256 256c-13.47 0-26.94-2.39-37.44-7.17l-148-67.49C63.79 178.26 48 169.25 48 152.24s15.79-26 22.58-29.12l149.28-68.07c20.57-9.4 51.61-9.4 72.19 0l149.37 68.07c6.79 3.09 22.58 12.1 22.58 29.12s-15.79 26-22.58 29.11l-148 67.48C282.94 253.61 269.47 256 256 256zm176.76-100.86z"
                />
                <path
                    fill="currentColor"
                    d="M441.36 226.81L426.27 220l-38.77 17.74-94 43c-10.5 4.8-24 7.19-37.44 7.19s-26.93-2.39-37.42-7.19l-94.07-43L85.79 220l-15.22 6.84C63.79 229.93 48 239 48 256s15.79 26.08 22.56 29.17l148 67.63C229 357.6 242.49 360 256 360s26.94-2.4 37.44-7.19l147.87-67.61c6.81-3.09 22.69-12.11 22.69-29.2s-15.77-26.07-22.64-29.19z"
                />
                <path
                    fill="currentColor"
                    d="M441.36 330.8l-15.09-6.8-38.77 17.73-94 42.95c-10.5 4.78-24 7.18-37.44 7.18s-26.93-2.39-37.42-7.18l-94.07-43L85.79 324l-15.22 6.84C63.79 333.93 48 343 48 360s15.79 26.07 22.56 29.15l148 67.59C229 461.52 242.54 464 256 464s26.88-2.48 37.38-7.27l147.92-67.57c6.82-3.08 22.7-12.1 22.7-29.16s-15.77-26.07-22.64-29.2z"
                />
            </svg>
        </button>

        <div class="btn" aria-label="more options" use:tooltip>
            <svg
                class="icon"
                xmlns="http://www.w3.org/2000/svg"
                width="512"
                height="512"
                viewBox="0 0 512 512"
            >
                <title>more options</title>
                <circle fill="currentColor" cx="256" cy="256" r="48" />
                <circle fill="currentColor" cx="416" cy="256" r="48" />
                <circle fill="currentColor" cx="96" cy="256" r="48" />
            </svg>
        </div>
    </div>
</section>

<div class="lang-stats flex">
    {#each langStats as lang}
        <div
            class="lang"
            style="width:{lang.percentage}%; background-color:{lang.language.color};"
            use:tooltip
            aria-label="{lang.language.name} {lang.percentage.toFixed(2)}%"
        />
    {/each}
</div>

<div class="pasties">
    {#if stackedView}
        {#each paste.pasties as pasty, i}
            <div class="pasty">
                <div class="title flex row space-between center">
                    <span>{pasty.title}</span>

                    <div class="meta-stacked flex row center">
                        <PastyMeta {pasty} {langStats} stats={pasteStats.pasties[pasty.id]} />
                    </div>
                </div>

                {@html highlightedCode[i]}
            </div>
        {/each}
    {:else}
        <div class="tabs flex row center">
            <div class="tabgroup flex row">
                {#each paste.pasties as pasty}
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

        <div class="meta-tabbed">
            <PastyMeta pasty={activePasty} {langStats} stats={pasteStats.pasties[activePastyId]} />
        </div>

        <!-- prettier-ignore -->
        {@html highlightedCode[paste.pasties.findIndex((p) => p.id === activePastyId)]}
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
                color: $color-bg-3;
                margin-top: 0.25rem;

                span:last-child {
                    white-space: pre;
                }
            }
        }

        .options {
            .btn {
                background-color: $color-bg;
                margin-left: 0.5rem;

                svg {
                    max-width: 20px;
                    max-height: 20px;
                }
            }

            .stars {
                p {
                    margin: 0;
                    margin-left: 0.5rem;
                }
            }
        }
    }

    .lang-stats {
        height: 5px;
        margin-bottom: 2rem;

        .lang {
            border-right: 2px solid $color-bg;

            &:first-child {
                border-bottom-left-radius: $border-radius;
            }

            &:last-child {
                border-bottom-right-radius: $border-radius;
                border-right: none;
            }
        }
    }

    .pasties {
        .tabs {
            width: 100%;
            box-sizing: border-box;
            background-color: $color-bg-2;
            border-radius: $border-radius $border-radius 0 0;
            border-bottom: 1px solid $color-bg-2;
            position: sticky;
            top: 0;
            z-index: 10;

            .tabgroup {
                flex-wrap: wrap;
                width: 100%;

                :global(.tab) {
                    flex-grow: 0;
                }
            }
        }

        .meta-tabbed {
            background-color: $color-bg-1;
            padding: 0.25rem;
            border: 1px solid $color-bg-2;
            border-top: none;
        }

        .pasty {
            margin-bottom: 2rem;

            &:last-child {
                margin-bottom: 0;
            }

            .title {
                background-color: $color-bg-1;
                border-radius: $border-radius $border-radius 0 0;
                border-bottom: 1px solid $color-prim;
                width: 100%;
                box-sizing: border-box;
                position: sticky;
                top: 0;
                z-index: 10;

                span {
                    padding: 0.5rem 1rem;
                }

                .meta-stacked {
                    padding-right: 1rem;
                }
            }
        }
    }

    :global(.shiki) {
        padding: 0.5rem;
        border-bottom-left-radius: $border-radius;
        border-bottom-right-radius: $border-radius;
        border: 1px solid $color-bg-2;
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
        color: $color-bg-3;
        font-size: $fs-normal;
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
                .btn:first-child {
                    margin-left: 0;
                }
            }
        }
    }
</style>
