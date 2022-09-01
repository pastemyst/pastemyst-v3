<script lang="ts">
    import { apiBase } from "$lib/api/api";
    import { getSelf } from "$lib/api/auth";
    import { currentUserStore } from "$lib/stores";
    import type { PageData } from "./$types";

    export let data: PageData;

    let avatarInput: HTMLInputElement;
    let avatarForm: HTMLFormElement;

    const onAvatarClick = () => {
        avatarInput.click();
    };

    const onAvatarChange = async () => {
        await fetch(`${apiBase}/settings/avatar`, {
            method: "PATCH",
            credentials: "include",
            body: new FormData(avatarForm)
        });

        const self = await getSelf(fetch);
        currentUserStore.set(self);
        data.self = self;
    };
</script>

<svelte:head>
    <title>pastemyst | profile settings</title>
</svelte:head>

<h3>profile settings</h3>

<p>settings for customization of your own profile.</p>

<h4>avatar</h4>

<div class="avatar">
    <img src={data.self?.avatarUrl} alt="{data.self?.username}'s avatar" />

    <button on:click={onAvatarClick}>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
            <path
                fill="currentColor"
                fill-rule="evenodd"
                d="M11.013 1.427a1.75 1.75 0 012.474 0l1.086 1.086a1.75 1.75 0 010 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 01-.927-.928l.929-3.25a1.75 1.75 0 01.445-.758l8.61-8.61zm1.414 1.06a.25.25 0 00-.354 0L10.811 3.75l1.439 1.44 1.263-1.263a.25.25 0 000-.354l-1.086-1.086zM11.189 6.25L9.75 4.81l-6.286 6.287a.25.25 0 00-.064.108l-.558 1.953 1.953-.558a.249.249 0 00.108-.064l6.286-6.286z"
            />
        </svg>

        edit
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

<style lang="scss">
    .avatar {
        position: relative;
        display: inline-block;
        max-width: 50%;
        margin-bottom: 2rem;

        &:hover button {
            opacity: 1;
        }

        img {
            border-radius: $border-radius;
            border: 1px solid var(--color-bg2);
            max-width: 100%;
            vertical-align: bottom;
        }

        button {
            position: absolute;
            top: 0;
            bottom: 0;
            left: 0;
            right: 0;
            width: 100%;
            background: rgba(20, 20, 20, 0.85);
            opacity: 0;
            display: flex;
            flex-direction: row;
            justify-content: center;
            align-items: center;
            @include transition(opacity);

            svg {
                margin-right: 0.5rem;
            }
        }
    }
</style>
