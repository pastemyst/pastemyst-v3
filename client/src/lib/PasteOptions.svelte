<script lang="ts">
    import { tooltip } from "$lib/tooltips";
    import { currentUserStore } from "./stores";

    interface Props {
        encrypt: boolean;
        isPrivate: boolean;
        pinned: boolean;
        anonymous: boolean;
        oncreatePaste: () => void;
    }

    let {
        encrypt = $bindable(false),
        isPrivate = $bindable(false),
        pinned = $bindable(false),
        anonymous = $bindable(false),
        oncreatePaste
    }: Props = $props();

    const onEncryptClick = () => {
        encrypt = !encrypt;
    };

    const onPrivateClick = () => {
        isPrivate = !isPrivate;

        if (isPrivate) {
            pinned = false;
            anonymous = false;
        }
    };

    const onPinClick = () => {
        pinned = !pinned;

        if (pinned) {
            isPrivate = false;
            anonymous = false;
        }
    };

    const onAnonymousClick = () => {
        anonymous = !anonymous;

        if (anonymous) {
            isPrivate = false;
            pinned = false;
        }
    };
</script>

<div class="paste-options block flex sm-row center space-between">
    <div class="options flex row center">
        <label
            for="encrypt"
            aria-label="encrypt the paste with a password"
            class="btn"
            class:checked={encrypt}
            use:tooltip
        >
            <input
                type="checkbox"
                name="encrypt"
                id="encrypt"
                onchange={onEncryptClick}
                bind:checked={encrypt}
            />
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Key Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M6.5 5.5a4 4 0 112.731 3.795.75.75 0 00-.768.18L7.44 10.5H6.25a.75.75 0 00-.75.75v1.19l-.06.06H4.25a.75.75 0 00-.75.75v1.19l-.06.06H1.75a.25.25 0 01-.25-.25v-1.69l5.024-5.023a.75.75 0 00.181-.768A3.995 3.995 0 016.5 5.5zm4-5.5a5.5 5.5 0 00-5.348 6.788L.22 11.72a.75.75 0 00-.22.53v2C0 15.216.784 16 1.75 16h2a.75.75 0 00.53-.22l.5-.5a.75.75 0 00.22-.53V14h.75a.75.75 0 00.53-.22l.5-.5a.75.75 0 00.22-.53V12h.75a.75.75 0 00.53-.22l.932-.932A5.5 5.5 0 1010.5 0zm.5 6a1 1 0 100-2 1 1 0 000 2z"
                />
            </svg>
        </label>

        {#if $currentUserStore != null}
            <label
                for="anonymous"
                aria-label="anonymous paste, won't be associated with your account"
                class="btn"
                class:checked={anonymous}
                use:tooltip
            >
                <input
                    type="checkbox"
                    name="anonymous"
                    id="anonymous"
                    onchange={onAnonymousClick}
                    bind:checked={anonymous}
                />
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>User Icon</title>
                    <path
                        fill="currentColor"
                        d="M4.243 4.757a3.757 3.757 0 115.851 3.119 6.006 6.006 0 013.9 5.339.75.75 0 01-.715.784H2.721a.75.75 0 01-.714-.784 6.006 6.006 0 013.9-5.34 3.753 3.753 0 01-1.664-3.118z"
                    />
                </svg>
            </label>

            <label
                for="private"
                aria-label="private paste, only visible by you"
                class="btn"
                class:checked={isPrivate}
                use:tooltip
            >
                <input
                    type="checkbox"
                    name="private"
                    id="private"
                    onchange={onPrivateClick}
                    bind:checked={isPrivate}
                />
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Lock Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M4 4v2h-.25A1.75 1.75 0 002 7.75v5.5c0 .966.784 1.75 1.75 1.75h8.5A1.75 1.75 0 0014 13.25v-5.5A1.75 1.75 0 0012.25 6H12V4a4 4 0 10-8 0zm6.5 2V4a2.5 2.5 0 00-5 0v2h5zM12 7.5h.25a.25.25 0 01.25.25v5.5a.25.25 0 01-.25.25h-8.5a.25.25 0 01-.25-.25v-5.5a.25.25 0 01.25-.25H12z"
                    />
                </svg>
            </label>

            <label
                for="pin"
                aria-label="pin the paste on your profile"
                class="btn"
                class:checked={pinned}
                use:tooltip
            >
                <input
                    type="checkbox"
                    name="pin"
                    id="pin"
                    onchange={onPinClick}
                    bind:checked={pinned}
                />
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Pin Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M4.456.734a1.75 1.75 0 012.826.504l.613 1.327a3.081 3.081 0 002.084 1.707l2.454.584c1.332.317 1.8 1.972.832 2.94L11.06 10l3.72 3.72a.75.75 0 11-1.061 1.06L10 11.06l-2.204 2.205c-.968.968-2.623.5-2.94-.832l-.584-2.454a3.081 3.081 0 00-1.707-2.084l-1.327-.613a1.75 1.75 0 01-.504-2.826L4.456.734zM5.92 1.866a.25.25 0 00-.404-.072L1.794 5.516a.25.25 0 00.072.404l1.328.613A4.582 4.582 0 015.73 9.63l.584 2.454a.25.25 0 00.42.12l5.47-5.47a.25.25 0 00-.12-.42L9.63 5.73a4.581 4.581 0 01-3.098-2.537L5.92 1.866z"
                    />
                </svg>
            </label>
        {/if}
    </div>

    <button class="btn-main" onclick={oncreatePaste}>create paste</button>
</div>

<style lang="scss">
    .paste-options {
        padding: 0.5rem;
    }

    @media screen and (max-width: $break-med) {
        .options {
            margin-bottom: 1rem;
        }
    }

    label {
        cursor: pointer;
        background-color: var(--color-bg);
        padding: 0.5rem;
        margin-right: 1rem;

        &:focus-within {
            border-color: var(--color-primary) !important;
        }

        &.checked {
            border-color: var(--color-secondary);
        }

        input[type="checkbox"] {
            appearance: none;
            width: 0;
            height: 0;

            &:focus-visible {
                outline: none;
                border: none;
            }

            &:checked ~ svg {
                color: var(--color-secondary);
            }
        }

        svg {
            @include transition(color);
        }
    }

    button {
        padding: 0.5rem 1rem;
    }
</style>
