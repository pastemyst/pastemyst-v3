<script lang="ts">
    import { createEventDispatcher } from "svelte";

    export let label: string;
    export let checked: boolean;

    const dispatch = createEventDispatcher();
</script>

<label>
    <input type="checkbox" bind:checked on:change={() => dispatch("change")} />
    <span>{label}</span>
</label>

<style lang="scss">
    label {
        display: grid;
        grid-template-columns: 1em auto;
        gap: 0.5em;
        cursor: pointer;
    }

    input[type="checkbox"] {
        appearance: none;
        background-color: var(--color-bg);
        margin: 0;
        font: inherit;
        color: currentColor;
        width: 1.15em;
        height: 1.15em;
        border: 1px solid var(--color-bg2);
        border-radius: $border-radius;
        transform: translateY(-0.075em);
        display: grid;
        place-content: center;
        cursor: pointer;
        @include transition(border-color, background-color);

        &:hover {
            border-color: var(--color-bg3);
            background-color: var(--color-bg2);
        }

        &::before {
            content: "";
            width: 0.65em;
            height: 0.65em;
            opacity: 0;
            box-shadow: inset 1em 1em var(--color-primary);
            background-color: CanvasText;
            transform-origin: bottom-left;
            clip-path: polygon(14% 44%, 0 65%, 50% 100%, 100% 16%, 80% 0%, 43% 62%);
            @include transition(opacity);
        }

        &:checked::before {
            opacity: 1;
        }

        &:focus {
            border-color: var(--color-primary);
        }
    }

    span {
        margin-left: 0.5rem;
        margin-top: -1px;
    }
</style>
