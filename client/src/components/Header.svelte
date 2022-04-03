<script lang="ts">
    import { currentUserStore, isCommandPaletteOpen } from "../stores";
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
            <a href="/login" class="btn btn-square nav-item" aria-label="login/register">
                <ion-icon name="person-circle" />
            </a>
        {/if}

        <button class="nav-item btn-square" on:click={() => isCommandPaletteOpen.set(true)} aria-label="menu">
            <ion-icon name="menu" />
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

            img {
                max-width: 24px;
                margin: 0;
                margin-left: 0.5rem;
                border-radius: $border-radius;
            }
        }
    }
</style>
