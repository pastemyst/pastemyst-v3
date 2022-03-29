<script lang="ts">
    import { currentUserStore, isCommandPaletteOpen } from "../stores";
</script>

<header class="flex row center space-between">
    <div class="flex row center">
        <img src="/images/pastemyst.svg" alt="pastemyst logo">
        <h1><a href="/" class="no-dec">pastemyst</a></h1>
    </div>

    <div class="flex row center">
        {#if $currentUserStore != null}
            <a href="/user/profile" class="nav-item user">
                {$currentUserStore.username}
                <img src="{$currentUserStore.avatarUrl}" alt="{$currentUserStore.username}'s avatar">
            </a>
        {:else}
            <a href="/login" class="nav-item" aria-label="login/register">
                <ion-icon name="person-circle" />
            </a>
        {/if}

        <button class="nav-item" on:click={() => isCommandPaletteOpen.set(true)}>
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

    button.nav-item {
        padding: 0;
        background-color: initial;
        color: $color-prim;
    }

    .nav-item {
        cursor: pointer;
        font-size: 1.75rem;
        @include transition;
        margin-left: 1rem;
        text-decoration: none;
        display: flex;
        align-items: center;

        &:hover {
            color: $color-sec;
        }

        &.user {
            font-size: $fs-normal;

            img {
                margin-left: 0.5rem;
                margin-right: 0;
                max-width: 35px;
                border-radius: $border-radius;
            }
        }
    }
</style>
