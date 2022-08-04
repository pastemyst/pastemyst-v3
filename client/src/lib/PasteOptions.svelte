<script lang="ts">
    import { tooltip } from "$lib/tooltips";
    import { createEventDispatcher } from "svelte";
    import { currentUserStore } from "./stores";

    export let encrypt = false;
    export let isPrivate = false;
    export let showOnProfile = false;
    export let anonymous = false;

    let dispatcher = createEventDispatcher();

    const onCreatePaste = () => {
        dispatcher("create");
    };

    const onEncryptClick = () => {
        encrypt = !encrypt;
    };

    const onPrivateClick = () => {
        isPrivate = !isPrivate;

        if (isPrivate) {
            showOnProfile = false;
            anonymous = false;
        }
    };

    const onShowOnProfileClick = () => {
        showOnProfile = !showOnProfile;

        if (showOnProfile) {
            isPrivate = false;
            anonymous = false;
        }
    };

    const onAnonymousClick = () => {
        anonymous = !anonymous;

        if (anonymous) {
            isPrivate = false;
            showOnProfile = false;
        }
    };
</script>

<div class="paste-options block flex sm-row center space-between">
    <div class="options flex row center">
        <input type="checkbox" id="encrypt" />
        <label
            for="encrypt"
            aria-label="encrypt the paste with a password"
            class="btn btn-square"
            class:enabled={encrypt}
            on:click={onEncryptClick}
            use:tooltip
        >
            <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                <title>Key</title>
                <path
                    fill="currentColor"
                    d="M218.1 167.17c0 13 0 25.6 4.1 37.4-43.1 50.6-156.9 184.3-167.5 194.5a20.17 20.17 0 00-6.7 15c0 8.5 5.2 16.7 9.6 21.3 6.6 6.9 34.8 33 40 28 15.4-15 18.5-19 24.8-25.2 9.5-9.3-1-28.3 2.3-36s6.8-9.2 12.5-10.4 15.8 2.9 23.7 3c8.3.1 12.8-3.4 19-9.2 5-4.6 8.6-8.9 8.7-15.6.2-9-12.8-20.9-3.1-30.4s23.7 6.2 34 5 22.8-15.5 24.1-21.6-11.7-21.8-9.7-30.7c.7-3 6.8-10 11.4-11s25 6.9 29.6 5.9c5.6-1.2 12.1-7.1 17.4-10.4 15.5 6.7 29.6 9.4 47.7 9.4 68.5 0 124-53.4 124-119.2S408.5 48 340 48s-121.9 53.37-121.9 119.17zM400 144a32 32 0 11-32-32 32 32 0 0132 32z"
                />
            </svg>
        </label>

        {#if $currentUserStore != null}
            <input type="checkbox" id="private" />
            <label
                for="private"
                aria-label="private paste, only visible by you"
                class="btn btn-square"
                class:enabled={isPrivate}
                on:click={onPrivateClick}
                use:tooltip
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                    <title>Lock Closed</title>
                    <path
                        fill="currentColor"
                        d="M368 192h-16v-80a96 96 0 10-192 0v80h-16a64.07 64.07 0 00-64 64v176a64.07 64.07 0 0064 64h224a64.07 64.07 0 0064-64V256a64.07 64.07 0 00-64-64zm-48 0H192v-80a64 64 0 11128 0z"
                    />
                </svg>
            </label>

            <input type="checkbox" id="show-on-profile" />
            <label
                for="show-on-profile"
                aria-label="show on your public profile"
                class="btn btn-square"
                class:enabled={showOnProfile}
                on:click={onShowOnProfileClick}
                use:tooltip
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                    <title>Eye</title>
                    <circle fill="currentColor" cx="256" cy="256" r="64" />
                    <path
                        fill="currentColor"
                        d="M490.84 238.6c-26.46-40.92-60.79-75.68-99.27-100.53C349 110.55 302 96 255.66 96c-42.52 0-84.33 12.15-124.27 36.11-40.73 24.43-77.63 60.12-109.68 106.07a31.92 31.92 0 00-.64 35.54c26.41 41.33 60.4 76.14 98.28 100.65C162 402 207.9 416 255.66 416c46.71 0 93.81-14.43 136.2-41.72 38.46-24.77 72.72-59.66 99.08-100.92a32.2 32.2 0 00-.1-34.76zM256 352a96 96 0 1196-96 96.11 96.11 0 01-96 96z"
                    />
                </svg>
            </label>

            <input type="checkbox" id="detach" />
            <label
                for="anonymous"
                aria-label="anonymous paste, won't be associated with your account"
                class="btn btn-square"
                class:enabled={anonymous}
                on:click={onAnonymousClick}
                use:tooltip
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                    <title>Glasses</title>
                    <path
                        fill="currentColor"
                        d="M464 184h-10.9a78.72 78.72 0 00-16-7.18C419.5 171 396.26 168 368 168s-51.5 3-69.06 8.82c-14.06 4.69-20.25 9.86-22.25 11.87a47.94 47.94 0 00-41.36 0c-2-2-8.19-7.18-22.25-11.87C195.5 171 172.26 168 144 168s-51.5 3-69.06 8.82a78.72 78.72 0 00-16 7.18H48a16 16 0 000 32h.17c1 45.46 6.44 72.78 18.11 92.23a66.78 66.78 0 0031.92 28c12.23 5.24 27.22 7.79 45.8 7.79 24.15 0 58.48-3.71 77.72-35.77 9.68-16.14 15.09-37.69 17.21-70.52A16 16 0 00240 232a16 16 0 0132 0 16 16 0 001.07 5.71c2.12 32.83 7.53 54.38 17.21 70.52a66.78 66.78 0 0031.92 28c12.23 5.24 27.22 7.79 45.8 7.79 24.15 0 58.48-3.71 77.72-35.77 11.67-19.45 17.13-46.77 18.11-92.23h.17a16 16 0 000-32z"
                    />
                </svg>
            </label>
        {/if}
    </div>

    <button class="btn-main" on:click={onCreatePaste}>create paste</button>
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

    input[type="checkbox"] {
        width: 0;
        height: 0;
        opacity: 0;

        &:checked + label {
            color: $color-prim;
        }
    }

    label {
        margin-right: 1rem;
        background-color: $color-bg;
        padding: 0.5rem;

        &:hover {
            background-color: $color-bg;
        }

        .icon {
            max-width: 20px;
        }

        &.enabled {
            border-color: $color-sec;

            .icon {
                color: $color-sec;
            }
        }
    }

    button {
        padding: 0.5rem 1rem;
    }
</style>
