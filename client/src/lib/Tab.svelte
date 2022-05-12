<script lang="ts">
    import { createEventDispatcher, tick } from "svelte";

    export let id: string;
    export let isActive: boolean = false;
    export let title: string = "untitled";
    export let isInRenamingState: boolean = false;
    export let isReadonly: boolean = false;
    export let closeable: boolean = true;

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
        onInput();
    };

    const onInputBlur = () => {
        isInRenamingState = false;
    };

    const onInput = () => {
        inputElement.style.width = inputElement.value.length + "ch";
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
        />
    {:else}
        <span class="title">{title}</span>
    {/if}

    {#if !isReadonly && closeable}
        <span class="close-icon flex center" on:click={onClose}>
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="currentColor"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    d="M6.2253 4.81108C5.83477 4.42056 5.20161 4.42056 4.81108 4.81108C4.42056 5.20161 4.42056 5.83477 4.81108 6.2253L10.5858 12L4.81114 17.7747C4.42062 18.1652 4.42062 18.7984 4.81114 19.1889C5.20167 19.5794 5.83483 19.5794 6.22535 19.1889L12 13.4142L17.7747 19.1889C18.1652 19.5794 18.7984 19.5794 19.1889 19.1889C19.5794 18.7984 19.5794 18.1652 19.1889 17.7747L13.4142 12L19.189 6.2253C19.5795 5.83477 19.5795 5.20161 19.189 4.81108C18.7985 4.42056 18.1653 4.42056 17.7748 4.81108L12 10.5858L6.2253 4.81108Z"
                    fill="currentColor"
                />
            </svg>
        </span>
    {/if}
</div>

<style lang="scss">
    .tab {
        background-color: $color-bg-1;
        cursor: pointer;
        user-select: none;
        white-space: nowrap;
        flex-grow: 0.25;
        border: 1px solid $color-bg-2;
        border-bottom-color: transparent;
        border-left-color: transparent;
        @include transition(border-color, background-color);

        &:first-child {
            border-radius: $border-radius 0 0 0;
            border-left-color: $color-bg-2;
        }

        &:hover {
            background-color: $color-bg-2;
            border-color: $color-bg-3;
        }

        &.active {
            border-color: $color-prim;
            z-index: 1;
        }

        .title {
            font-size: $fs-normal;
            margin: 0.5rem 1rem;
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
                color: $color-red;
            }
        }

        input {
            background-color: transparent;
            border-radius: $border-radius;
            color: $color-sec;
            border: none;
            outline: none;
            width: auto;
            padding: 0;
            font-size: $fs-normal;
            transition: none;
        }
    }
</style>
