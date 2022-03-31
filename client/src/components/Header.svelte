<script lang="ts">
    import { currentUserStore, isCommandPaletteOpen } from "../stores";
</script>

<header class="flex row center space-between">
    <div class="flex row center">
        <img src="/images/pastemyst.svg" alt="pastemyst logo" />
        <h1><a href="/" class="no-dec">pastemyst</a></h1>
    </div>

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

        <button class="nav-item btn-square" on:click={() => isCommandPaletteOpen.set(true)}>
            <ion-icon name="menu" />
        </button>
    </div>
</header>

<style lang="scss">
    header {
        margin-top: 1rem;
        flex-shrink: 0;
    }

    img {
        width: 50px;
        margin-right: 1rem;
    }

    h1 {
        margin: 0;
        margin-top: -3px;
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
