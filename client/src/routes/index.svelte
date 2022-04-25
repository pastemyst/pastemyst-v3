<script lang="ts">
    import PasteOptions from "$lib/PasteOptions.svelte";
    import TabbedEditor from "$lib/TabbedEditor.svelte";

    let expiresIn = "never";
</script>

<div class="title-input flex sm-row">
    <label class="hidden" for="paste-title">paste title</label>
    <input type="text" placeholder="title" id="paste-title" name="paste-title" />

    <button>expires in: {expiresIn}</button>
</div>

<TabbedEditor />

<div class="paste-options">
    <PasteOptions />
</div>

<style lang="scss">
    .title-input {
        margin-top: 2rem;
        margin-bottom: 2rem;

        input {
            width: 100%;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }

        button {
            border-top-left-radius: 0;
            border-bottom-left-radius: 0;
            white-space: nowrap;
            padding: 0.5rem 1rem;
            border-left-color: $color-bg-1;

            &:hover {
                border-left-color: $color-bg-3;
            }

            &:focus,
            &:active {
                border-left-color: $color-prim;
            }
        }

        @media screen and (max-width: $break-med) {
            input {
                border-radius: $border-radius;
                border-bottom-left-radius: 0;
                border-bottom-right-radius: 0;
            }

            button {
                border-radius: $border-radius;
                border-top-left-radius: 0;
                border-top-right-radius: 0;
                text-align: left;
                border-left-color: $color-bg-2;
                border-top-color: $color-bg-1;

                &:hover {
                    border-top-color: $color-bg-3;
                }

                &:focus,
                &:active {
                    border-top-color: $color-prim;
                }
            }
        }
    }

    .paste-options {
        margin-top: 2rem;
    }
</style>
