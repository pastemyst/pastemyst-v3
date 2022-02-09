<div id="header" class="flex inline center space-between">
    <div class="flex inline center">
        <img src="/images/pastemyst.svg" alt="pastemyst logo">
        <h1><a href="/" class="no-dec">pastemyst</a></h1>
    </div>

    <div class="flex inline center">
        <ion-icon name="person-circle" />

        <ion-icon name="menu" />
    </div>
</div>

<style lang="scss">
    #header {
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

    ion-icon {
        cursor: pointer;
        font-size: 1.75rem;
        @include transition;
        margin-left: 1rem;

        &:hover {
            color: $color-sec;
        }
    }
</style>