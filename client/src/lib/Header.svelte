<script lang="ts">
    import { currentUserStore } from "./stores";
    import { tooltip } from "./tooltips";

    const onMenuClick = () => {
        window.dispatchEvent(new CustomEvent("openCmd"));
    };
</script>

<header class="flex row center space-between">
    <a href="/" class="title btn">
        <img src="/images/pastemyst.svg" alt="pastemyst logo" />
        <h1>pastemyst</h1>
    </a>

    <div class="flex row center">
        {#if $currentUserStore != null}
            <a href="/user/profile" class="nav-item user btn">
                {$currentUserStore.username}
                <img
                    src={$currentUserStore.avatarUrl}
                    alt="{$currentUserStore.username}'s avatar"
                />
            </a>
        {:else}
            <a
                href="/login"
                class="btn btn-square nav-item"
                aria-label="login/register"
                use:tooltip
            >
                <svg
                    width="24"
                    height="24"
                    viewBox="0 0 24 24"
                    fill="none"
                    xmlns="http://www.w3.org/2000/svg"
                    class="icon"
                >
                    <title>user icon</title>
                    <path
                        fill-rule="evenodd"
                        clip-rule="evenodd"
                        d="M16 7C16 9.20914 14.2091 11 12 11C9.79086 11 8 9.20914 8 7C8 4.79086 9.79086 3 12 3C14.2091 3 16 4.79086 16 7ZM14 7C14 8.10457 13.1046 9 12 9C10.8954 9 10 8.10457 10 7C10 5.89543 10.8954 5 12 5C13.1046 5 14 5.89543 14 7Z"
                        fill="currentColor"
                    />
                    <path
                        d="M16 15C16 14.4477 15.5523 14 15 14H9C8.44772 14 8 14.4477 8 15V21H6V15C6 13.3431 7.34315 12 9 12H15C16.6569 12 18 13.3431 18 15V21H16V15Z"
                        fill="currentColor"
                    />
                </svg>
            </a>
        {/if}

        <button
            class="nav-item btn-square"
            on:click={onMenuClick}
            aria-label="menu"
            role="menu"
            use:tooltip
        >
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <title>menu icon</title>
                <path
                    d="M2 6C2 5.44772 2.44772 5 3 5H21C21.5523 5 22 5.44772 22 6C22 6.55228 21.5523 7 21 7H3C2.44772 7 2 6.55228 2 6Z"
                    fill="currentColor"
                />
                <path
                    d="M2 12.0322C2 11.4799 2.44772 11.0322 3 11.0322H21C21.5523 11.0322 22 11.4799 22 12.0322C22 12.5845 21.5523 13.0322 21 13.0322H3C2.44772 13.0322 2 12.5845 2 12.0322Z"
                    fill="currentColor"
                />
                <path
                    d="M3 17.0645C2.44772 17.0645 2 17.5122 2 18.0645C2 18.6167 2.44772 19.0645 3 19.0645H21C21.5523 19.0645 22 18.6167 22 18.0645C22 17.5122 21.5523 17.0645 21 17.0645H3Z"
                    fill="currentColor"
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
            color: $color-prim;
        }

        img {
            max-width: 24px;
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

            img {
                max-width: 24px;
                margin: 0;
                margin-left: 0.5rem;
                border-radius: $border-radius;
            }
        }
    }
</style>
