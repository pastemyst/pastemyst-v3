<script lang="ts">
    import {
        createAnnouncement,
        deleteAnnouncement,
        editAnnouncement,
        getAllAnnouncements
    } from "$lib/api/announcement";
    import { marked } from "marked";
    import type { PageData } from "./$types";
    import { formatDistanceToNow } from "date-fns";
    import { tooltip } from "$lib/tooltips";
    import { Close, getConfirmActionCommands, setTempCommands } from "$lib/command";
    import { cmdPalOpen, cmdPalTitle } from "$lib/stores";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let announcements = $state(data.announcements);
    let renderedMarkdown = $state(data.renderedMarkdown);

    let creatingAnnouncement = $state(false);
    let editingAnnouncementId = $state<string | undefined>(undefined);
    let title: string | undefined = $state(undefined);
    let content: string | undefined = $state(undefined);

    const onCreateAnnouncementStart = () => {
        creatingAnnouncement = true;
    };

    const onCreateAnnouncementCancel = () => {
        creatingAnnouncement = false;
    };

    const onCreateAnnouncement = async () => {
        if (!title || !content) {
            return;
        }

        await createAnnouncement(fetch, title, content);

        creatingAnnouncement = false;
        title = undefined;
        content = undefined;

        await refetchAnnouncements();
    };

    const refetchAnnouncements = async () => {
        announcements = await getAllAnnouncements(fetch);

        renderedMarkdown = [];
        for (let i = 0; i < announcements.length; i++) {
            renderedMarkdown.push(await marked.parse(announcements[i].content, { gfm: true }));
        }
    };

    const onDeleteAnnouncement = async (id: string) => {
        setTempCommands(
            getConfirmActionCommands(() => {
                (async () => {
                    await deleteAnnouncement(fetch, id);

                    await refetchAnnouncements();
                })();

                return Close.yes;
            })
        );

        cmdPalTitle.set("are you sure you want to delete this annoucement?");
        cmdPalOpen.set(true);
    };

    const onEditAnnouncementStart = (id: string, oldTitle: string, oldContent: string) => {
        title = oldTitle;
        content = oldContent;
        editingAnnouncementId = id;
        creatingAnnouncement = true;
    };

    const onEditAnnouncement = async () => {
        if (!title || !content || !editingAnnouncementId) {
            return;
        }

        await editAnnouncement(fetch, editingAnnouncementId, title, content);

        creatingAnnouncement = false;
        editingAnnouncementId = undefined;
        title = undefined;
        content = undefined;

        await refetchAnnouncements();
    };
</script>

<svelte:head>
    <title>pastemyst | admin</title>
</svelte:head>

<h3>admin settings</h3>

<p>various admin settings</p>

<h4>announcements</h4>

<button class="new-announcement-button" onclick={onCreateAnnouncementStart}
    >create new announcement</button
>

{#if creatingAnnouncement}
    <div class="new-announcement flex col block">
        <label for="title">title:</label>
        <input type="text" name="title" id="title" bind:value={title} />

        <label for="content">content (markdown):</label>
        <textarea name="content" id="content" rows="5" bind:value={content}></textarea>

        <div class="buttons flex row gap-m">
            {#if editingAnnouncementId}
                <button onclick={onEditAnnouncement}>edit</button>
            {:else}
                <button onclick={onCreateAnnouncement}>create</button>
            {/if}
            <button onclick={onCreateAnnouncementCancel}>cancel</button>
        </div>
    </div>
{/if}

{#each announcements as announcement, i}
    <div class="announcement">
        <div class="flex row space-between">
            <p class="title">{announcement.title}</p>

            <div class="flex row gap-s">
                <button
                    onclick={() =>
                        onEditAnnouncementStart(
                            announcement.id,
                            announcement.title,
                            announcement.content
                        )}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Pen Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M11.013 1.427a1.75 1.75 0 012.474 0l1.086 1.086a1.75 1.75 0 010 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 01-.927-.928l.929-3.25a1.75 1.75 0 01.445-.758l8.61-8.61zm1.414 1.06a.25.25 0 00-.354 0L10.811 3.75l1.439 1.44 1.263-1.263a.25.25 0 000-.354l-1.086-1.086zM11.189 6.25L9.75 4.81l-6.286 6.287a.25.25 0 00-.064.108l-.558 1.953 1.953-.558a.249.249 0 00.108-.064l6.286-6.286z"
                        />
                    </svg>
                </button>
                <button onclick={() => onDeleteAnnouncement(announcement.id)}>
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <title>Trash Icon</title>
                        <path
                            fill="currentColor"
                            fill-rule="evenodd"
                            d="M6.5 1.75a.25.25 0 01.25-.25h2.5a.25.25 0 01.25.25V3h-3V1.75zm4.5 0V3h2.25a.75.75 0 010 1.5H2.75a.75.75 0 010-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75zM4.496 6.675a.75.75 0 10-1.492.15l.66 6.6A1.75 1.75 0 005.405 15h5.19c.9 0 1.652-.681 1.741-1.576l.66-6.6a.75.75 0 00-1.492-.149l-.66 6.6a.25.25 0 01-.249.225h-5.19a.25.25 0 01-.249-.225l-.66-6.6z"
                        />
                    </svg>
                </button>
            </div>
        </div>
        <span class="date" use:tooltip aria-label={new Date(announcement.createdAt).toString()}
            >{formatDistanceToNow(new Date(announcement.createdAt), { addSuffix: true })}</span
        >
        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        <p>{@html renderedMarkdown[i]}</p>
    </div>
{/each}

<style lang="scss">
    .new-announcement-button {
        margin-bottom: 1rem;
    }

    .new-announcement {
        background-color: var(--color-bg);
        margin-bottom: 1rem;
        padding: 1rem;
        border-radius: $border-radius;
        border: 1px solid var(--color-bg2);
        font-size: $fs-normal;

        label {
            margin-bottom: 0.25rem;
        }

        input[type="text"],
        textarea {
            width: 100%;
            margin-bottom: 1rem;
            padding: 0.5rem;
            font-size: $fs-normal;
            background-color: var(--color-bg1);
        }

        button {
            background-color: var(--color-bg1);
        }
    }

    .announcement {
        background-color: var(--color-bg);
        margin-bottom: 1rem;
        padding: 0.5rem 1rem;
        border-radius: $border-radius;
        border: 1px solid var(--color-bg2);
        font-size: $fs-normal;

        .title {
            font-weight: bold;
            font-size: $fs-large;
            margin: 0;
        }

        .date {
            font-size: $fs-small;
            color: var(--color-bg3);
        }
    }
</style>
