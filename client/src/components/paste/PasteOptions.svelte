<div class="paste-options flex sm-row center space-between">
    <div class="options flex row center">
        <input type="checkbox" id="encrypt" />
        <label for="encrypt" class="material-icons" aria-label="encrypt the paste with a password">key</label>

        <input type="checkbox" id="private" />
        <label for="private" class="material-icons" aria-label="private paste, only visible by you">lock</label>

        <input type="checkbox" id="show-on-profile" />
        <label for="show-on-profile" class="material-icons" aria-label="show on your public profile">person</label>

        <input type="checkbox" id="detach" />
        <label for="detach" class="material-icons" aria-label="detach paste, won't be associated with your account" >fingerprint</label>
    </div>

    <button class="btn-main">create paste</button>
</div>

<style lang="scss">
    .paste-options {
        background-color: $color-bg-1;
        border-radius: $border-radius;
        padding: 0.5rem 1rem;
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
        cursor: pointer;
        @include transition();
        margin-right: 1rem;

        &:hover {
            color: $color-sec;
        }
    }
</style>