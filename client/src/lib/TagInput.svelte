<script lang="ts">
    import { tick } from "svelte";
    import { tooltip } from "./tooltips";

    export let anonymousPaste = false;
    export let tags: string[] = [];
    export let existingTags: string[];

    let addingTag = false;

    let addTagElement: HTMLButtonElement;
    let newTag: string;
    let tagInputElement: HTMLInputElement;

    const removeTag = (tag: string) => {
        const tagIndex = tags.findIndex((t) => t === tag);

        tags.splice(tagIndex, 1);
        tags = tags;
    };

    const onAddTag = async () => {
        if (anonymousPaste) return;

        addingTag = true;
        await tick();
        tagInputElement.focus();
    };

    const onTagInputBlur = () => {
        if (newTag && !tags.includes(newTag.trim())) {
            tags.push(newTag.trim());
            tags = tags;
        }

        newTag = "";
        addingTag = false;
    };

    const onTagInputKeyPress = (event: KeyboardEvent) => {
        if (event.key === "Enter") {
            onTagInputBlur();
            addTagElement.focus();
        } else if (event.key == "Escape") {
            newTag = "";
            onTagInputBlur();
            addTagElement.focus();
        }
    };
</script>

<div class="tags flex row center">
    <p class="btn">tags</p>

    {#each tags as tag}
        <div class="tag flex row center">
            <span>{tag}</span>
            <button on:click={() => removeTag(tag)}>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <path
                        fill="currentColor"
                        d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.749.749 0 0 1 1.275.326.749.749 0 0 1-.215.734L9.06 8l3.22 3.22a.749.749 0 0 1-.326 1.275.749.749 0 0 1-.734-.215L8 9.06l-3.22 3.22a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"
                    />
                </svg>
            </button>
        </div>
    {/each}

    {#if addingTag}
        <input
            type="text"
            placeholder="tag name..."
            bind:this={tagInputElement}
            bind:value={newTag}
            on:blur={onTagInputBlur}
            on:keydown={onTagInputKeyPress}
            list="taglist"
        />
        <datalist id="taglist">
            {#each existingTags as tag}
                <option value={tag}>{tag}</option>
            {/each}
        </datalist>
    {/if}

    <button
        class="add-tag flex row"
        aria-label={anonymousPaste ? "can't tag anonymous pastes" : "add tag"}
        use:tooltip
        on:click={onAddTag}
        bind:this={addTagElement}
        class:disabled={anonymousPaste}
    >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
            <path
                fill="currentColor"
                d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"
            />
        </svg>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
            <path
                fill="currentColor"
                d="M7.75 2a.75.75 0 0 1 .75.75V7h4.25a.75.75 0 0 1 0 1.5H8.5v4.25a.75.75 0 0 1-1.5 0V8.5H2.75a.75.75 0 0 1 0-1.5H7V2.75A.75.75 0 0 1 7.75 2Z"
            />
        </svg>
    </button>
</div>

<style lang="scss">
    .tags {
        margin-bottom: 1rem;
        gap: 0.5rem;
        font-size: $fs-small;
        flex-wrap: wrap;

        p {
            margin: 0;
            align-self: center;
            cursor: initial;

            &:hover {
                border-color: var(--color-bg2);
                background-color: var(--color-bg1);
            }
        }

        .tag {
            background-color: var(--color-bg1);
            padding: 0.25rem 0.5rem;
            border-radius: $border-radius;
            border: 1px solid var(--color-bg2);

            span {
                margin-right: 0.5rem;
            }

            button {
                background-color: transparent;
                border: none;
                @include transition(color);
                padding: 0;

                &:hover svg {
                    color: var(--color-danger);
                }

                svg {
                    width: $fs-small;
                    height: $fs-small;
                }
            }
        }

        input {
            padding: 0.25rem 0.5rem;
            width: 7rem;
        }

        .add-tag {
            svg {
                width: $fs-small;
                height: $fs-small;

                &:first-child {
                    margin-right: 0.5rem;
                }
            }
        }
    }
</style>
