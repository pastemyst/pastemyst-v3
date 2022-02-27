<script lang="ts">
    import { isCommandPaletteOpen } from "../stores";
</script>

<header class="flex row center space-between">
    <div class="flex row center">
        <img src="/images/pastemyst.svg" alt="pastemyst logo">
        <h1><a href="/" class="no-dec">pastemyst</a></h1>
    </div>

    <div class="flex row center">
        <span class="material-icons">account_circle</span>

        <span class="material-icons" on:click={() => isCommandPaletteOpen.set(true)}>menu</span>
    </div>
</header>

<style lang="scss">
    header {
        margin-top: 1rem;
    }

    img {
        width: 50px;
        margin-right: 1rem;
    }

    h1 {
        margin: 0;
        margin-top: -3px;
    }

    .material-icons {
        cursor: pointer;
        font-size: 1.75rem;
        @include transition;
        margin-left: 1rem;

        &:hover {
            color: $color-sec;
        }
    }
</style>
