<div class="paste-options block flex sm-row center space-between">
    <div class="options flex row center">
        <input type="checkbox" id="encrypt" />
        <label for="encrypt" aria-label="encrypt the paste with a password" class="btn btn-square">
            <ion-icon name="key" />
        </label>

        <input type="checkbox" id="private" />
        <label for="private" aria-label="private paste, only visible by you" class="btn btn-square">
            <ion-icon name="lock-closed" />
        </label>

        <input type="checkbox" id="show-on-profile" />
        <label for="show-on-profile" aria-label="show on your public profile" class="btn btn-square">
            <ion-icon name="people" />
        </label>

        <input type="checkbox" id="detach" />
        <label for="detach" aria-label="detach paste, won't be associated with your account" class="btn btn-square">
            <ion-icon name="finger-print" />
        </label>
    </div>

    <button class="btn-main">create paste</button>
</div>

<style lang="scss">
    .paste-options {
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
            border-color: $color-prim;
        }
    }

    label {
        margin-right: 1rem;
        font-size: $fs-large;
        background-color: $color-bg;
    }

    button {
        padding: 0.25rem 0.5rem;
    }
</style>
