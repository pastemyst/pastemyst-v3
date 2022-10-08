<script lang="ts">
    import { apiBase } from "$lib/api/api";
    import { getSelf } from "$lib/api/auth";
    import { doesUserExist } from "$lib/api/user";
    import { usernameRegex } from "$lib/patterns";
    import { currentUserStore } from "$lib/stores";
    import type { PageData } from "./$types";

    export let data: PageData;

    let avatarInput: HTMLInputElement;
    let avatarForm: HTMLFormElement;

    let usernameInputValue = data.self.username;
    let usernameAvailable = true;
    let usernameValid = true;
    let waitingForUsernameCheck = false;

    const onAvatarEditClick = () => {
        avatarInput.click();
    };

    const onAvatarChange = async () => {
        await fetch(`${apiBase}/settings/avatar`, {
            method: "PATCH",
            credentials: "include",
            body: new FormData(avatarForm)
        });

        const self = await getSelf(fetch);

        if (self) {
            currentUserStore.set(self);
            data.self = self;
        }
    };

    let usernameTimer: number;

    const onUsernameInputKeyup = () => {
        waitingForUsernameCheck = true;
        window.clearTimeout(usernameTimer);
        usernameTimer = window.setTimeout(checkIfUsernameAvailable, 250);
    };

    const checkIfUsernameAvailable = async () => {
        if (usernameInputValue === data.self.username) {
            usernameAvailable = true;
            waitingForUsernameCheck = false;
            return;
        }

        usernameValid = usernameRegex.test(usernameInputValue);

        usernameAvailable = !(await doesUserExist(usernameInputValue));

        waitingForUsernameCheck = false;
    };

    const onUsernameSave = async () => {
        if (
            waitingForUsernameCheck ||
            !usernameAvailable ||
            data.self.username === usernameInputValue
        )
            return;

        waitingForUsernameCheck = true;

        const usernameData = {
            username: usernameInputValue
        };

        await fetch(`${apiBase}/settings/username`, {
            method: "PATCH",
            credentials: "include",
            body: JSON.stringify(usernameData),
            headers: {
                "Content-Type": "application/json"
            }
        });

        const self = await getSelf(fetch);

        if (self) {
            currentUserStore.set(self);
            data.self = self;
        }

        await checkIfUsernameAvailable();
    };
</script>

<svelte:head>
    <title>pastemyst | profile settings</title>
</svelte:head>

<h3>profile settings</h3>

<p>settings for customization of your own profile.</p>

<h4>avatar</h4>

<div class="avatar flex sm-row center">
    <img src={data.self?.avatarUrl} alt="{data.self.username}'s avatar" />

    <button on:click={onAvatarEditClick}>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
            <path
                fill="currentColor"
                fill-rule="evenodd"
                d="M11.013 1.427a1.75 1.75 0 012.474 0l1.086 1.086a1.75 1.75 0 010 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 01-.927-.928l.929-3.25a1.75 1.75 0 01.445-.758l8.61-8.61zm1.414 1.06a.25.25 0 00-.354 0L10.811 3.75l1.439 1.44 1.263-1.263a.25.25 0 000-.354l-1.086-1.086zM11.189 6.25L9.75 4.81l-6.286 6.287a.25.25 0 00-.064.108l-.558 1.953 1.953-.558a.249.249 0 00.108-.064l6.286-6.286z"
            />
        </svg>

        <span>edit</span>
    </button>

    <form class="hidden" bind:this={avatarForm}>
        <input
            type="file"
            name="file"
            id="avatar"
            accept="image/*"
            bind:this={avatarInput}
            on:change={onAvatarChange}
        />
    </form>
</div>

<h4>username</h4>

<div class="username flex sm-row">
    <input
        type="text"
        bind:value={usernameInputValue}
        on:keyup={onUsernameInputKeyup}
        required
        name="username"
        id="username"
        placeholder="username..."
        maxlength="20"
        minlength="1"
    />

    <button
        disabled={waitingForUsernameCheck ||
            !usernameAvailable ||
            data.self.username === usernameInputValue}
        on:click={onUsernameSave}
    >
        {#if waitingForUsernameCheck}
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon loader">
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M8 2.5a5.487 5.487 0 00-4.131 1.869l1.204 1.204A.25.25 0 014.896 6H1.25A.25.25 0 011 5.75V2.104a.25.25 0 01.427-.177l1.38 1.38A7.001 7.001 0 0114.95 7.16a.75.75 0 11-1.49.178A5.501 5.501 0 008 2.5zM1.705 8.005a.75.75 0 01.834.656 5.501 5.501 0 009.592 2.97l-1.204-1.204a.25.25 0 01.177-.427h3.646a.25.25 0 01.25.25v3.646a.25.25 0 01-.427.177l-1.38-1.38A7.001 7.001 0 011.05 8.84a.75.75 0 01.656-.834z"
                />
            </svg>
        {:else if usernameAvailable}
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon available">
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M8 16A8 8 0 108 0a8 8 0 000 16zm3.78-9.72a.75.75 0 00-1.06-1.06L6.75 9.19 5.28 7.72a.75.75 0 00-1.06 1.06l2 2a.75.75 0 001.06 0l4.5-4.5z"
                />
            </svg>
        {:else}
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon taken">
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M2.343 13.657A8 8 0 1113.657 2.343 8 8 0 012.343 13.657zM6.03 4.97a.75.75 0 00-1.06 1.06L6.94 8 4.97 9.97a.75.75 0 101.06 1.06L8 9.06l1.97 1.97a.75.75 0 101.06-1.06L9.06 8l1.97-1.97a.75.75 0 10-1.06-1.06L8 6.94 6.03 4.97z"
                />
            </svg>
        {/if}

        <span>save</span>
    </button>

    {#if !usernameValid}
        <p class="error">1-20 chars, only a-z and .,-_</p>
    {/if}
</div>

<style lang="scss">
    .avatar {
        gap: 1rem;

        img {
            max-width: 10rem;
            border-radius: $border-radius;
        }
    }

    .username {
        margin-bottom: 2rem;
        gap: 1rem;
        flex-flow: wrap;

        input {
            padding: 0.25rem 0.5rem;
        }

        .icon {
            &.taken {
                color: var(--color-danger);
            }

            &.available {
                color: var(--color-success);
            }

            &.loader {
                animation: loader 1.2s linear infinite;
            }
        }

        .error {
            width: 100%;
            color: var(--color-danger);
            font-size: $fs-small;
            margin: 0;
            margin-top: -0.5rem;
            margin-left: 0.5rem;
        }
    }

    button {
        .icon {
            margin-right: 0.5rem;
        }
    }

    @keyframes loader {
        0% {
            transform: rotate(360deg);
        }

        100% {
            transform: rotate(0deg);
        }
    }
</style>
