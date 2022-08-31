<script lang="ts">
    import { createEventDispatcher, tick } from "svelte";

    export let id: string;
    export let isActive = false;
    export let title = "untitled";
    export let isInRenamingState = false;
    export let isReadonly = false;
    export let closeable = true;

    let dispatch = createEventDispatcher();

    let inputElement: HTMLInputElement;

    const onClick = (event: MouseEvent) => {
        dispatch("click", { event: event });
    };

    const onDblClick = async () => {
        if (isReadonly) return;

        isInRenamingState = true;

        await tick();

        inputElement.focus();
        inputElement.select();
        onInput();
    };

    const onInputBlur = () => {
        isInRenamingState = false;

        // don't allow empty pasty titles
        if (title.length === 0) title = "untitled";

        dispatch("finishedRenaming");
    };

    const onInput = () => {
        inputElement.style.width = inputElement.value.length + "ch";

        if (title.length === 0) {
            inputElement.style.width = inputElement.placeholder.length + "ch";
        }
    };

    const onInputKeyup = (event: KeyboardEvent) => {
        if (event.key === "Enter" || event.key === "Escape") onInputBlur();
    };

    const onClose = () => {
        dispatch("close");
    };
</script>

<div
    class="tab flex row center space-between"
    class:active={isActive}
    class:rename-state={isInRenamingState}
    data-id={id}
    on:click={onClick}
    on:dblclick={onDblClick}
>
    {#if isInRenamingState}
        <input
            type="text"
            bind:value={title}
            on:blur={onInputBlur}
            on:input={onInput}
            on:keyup={onInputKeyup}
            bind:this={inputElement}
            placeholder="untitled"
            maxlength="50"
        />
    {:else}
        <span class="title">{title}</span>
    {/if}

    {#if !isReadonly && closeable}
        <span class="close-icon flex center" on:click={onClose}>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Close Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M3.72 3.72a.75.75 0 011.06 0L8 6.94l3.22-3.22a.75.75 0 111.06 1.06L9.06 8l3.22 3.22a.75.75 0 11-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 01-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 010-1.06z"
                />
            </svg>
        </span>
    {/if}
</div>

<style lang="scss">
    .tab {
        background-color: var(--color-bg1);
        cursor: pointer;
        user-select: none;
        flex-grow: 0.25;
        border: 1px solid var(--color-bg2);
        border-bottom-color: transparent;
        border-left-color: transparent;
        @include transition(border-color, background-color);

        &:first-child {
            border-radius: $border-radius 0 0 0;
            border-left-color: var(--color-bg2);
        }

        &:hover {
            background-color: var(--color-bg2);
            border-color: var(--color-bg3);
        }

        &.active {
            border-color: var(--color-primary);
            z-index: 1;
        }

        .title {
            font-size: $fs-normal;
            margin: 0.5rem 1rem;
            word-break: break-word;
        }

        .close-icon {
            border-radius: $border-radius;
            height: 100%;
            width: 2rem;
            margin-right: 0.25rem;
            margin-left: -1rem;

            .icon {
                max-width: 15px;
                max-height: 15px;
                margin-left: auto;
                margin-right: auto;
                @include transition(color);
            }

            &:hover .icon {
                color: var(--color-danger);
            }
        }

        input {
            background-color: transparent;
            border-radius: $border-radius;
            color: var(--color-secondary);
            border: none;
            outline: none;
            width: auto;
            padding: 0;
            margin: 0.5rem 1rem;
            font-size: $fs-normal;
            transition: none;
        }
    }
</style>
