<script lang="ts">
    import { cmdPalOpen, currentUserStore } from "./stores";
    import { tooltip } from "./tooltips";

    const onMenuClick = () => {
        cmdPalOpen.set(true);
    };
</script>

<header class="flex row center space-between">
    <a href="/" class="title btn">
        <img src="/images/pastemyst.svg" alt="pastemyst logo" />
        <h1>pastemyst</h1>
    </a>

    <div class="flex row center">
        {#if $currentUserStore != null}
            <a href="/~{$currentUserStore.username}" class="nav-item user btn btn-icon">
                <img
                    src={$currentUserStore.avatarUrl}
                    alt="{$currentUserStore.username}'s avatar"
                />
            </a>
        {:else}
            <a href="/login" class="btn btn-icon nav-item" aria-label="login/register" use:tooltip>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Login Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M2 2.75C2 1.784 2.784 1 3.75 1h2.5a.75.75 0 010 1.5h-2.5a.25.25 0 00-.25.25v10.5c0 .138.112.25.25.25h2.5a.75.75 0 010 1.5h-2.5A1.75 1.75 0 012 13.25V2.75zm6.56 4.5l1.97-1.97a.75.75 0 10-1.06-1.06L6.22 7.47a.75.75 0 000 1.06l3.25 3.25a.75.75 0 101.06-1.06L8.56 8.75h5.69a.75.75 0 000-1.5H8.56z"
                    />
                </svg>
            </a>
        {/if}

        <button
            class="nav-item btn-icon"
            on:click={onMenuClick}
            aria-label="menu"
            role="menu"
            use:tooltip
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Menu Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M1 2.75A.75.75 0 011.75 2h12.5a.75.75 0 110 1.5H1.75A.75.75 0 011 2.75zm0 5A.75.75 0 011.75 7h12.5a.75.75 0 110 1.5H1.75A.75.75 0 011 7.75zM1.75 12a.75.75 0 100 1.5h12.5a.75.75 0 100-1.5H1.75z"
                />
            </svg>
        </button>
    </div>
</header>

<style lang="scss">
    header {
        margin-top: 1rem;
        flex-shrink: 0;
    }

    .title {
        text-decoration: none;

        h1 {
            font-size: $fs-normal;
            font-weight: normal;
            margin: 0;
            color: var(--color-primary);
        }

        img {
            width: 24px;
            height: 24px;
            margin: 0;
            margin-right: 0.5rem;
        }
    }

    .nav-item {
        font-size: $fs-large;
        margin-left: 1rem;

        &.user {
            font-size: $fs-normal;
            text-decoration: none;
            word-break: break-word;
            padding: 0;

            img {
                width: 34px;
                height: 34px;
                margin: 0;
                border-radius: $border-radius;
            }
        }
    }
</style>
