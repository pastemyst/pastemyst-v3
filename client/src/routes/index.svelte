<script lang="ts">
    let expiresIn = "never";
</script>

<div class="title-input flex sm-row">
    <label class="hidden" for="paste-title">paste title</label>
    <input type="text" placeholder="title" id="paste-title" name="paste-title" />

    <button>expires in: {expiresIn}</button>
</div>

<style lang="scss">
    .title-input {
        margin-top: 2rem;

        input {
            width: 100%;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }

        button {
            border-top-left-radius: 0;
            border-bottom-left-radius: 0;
            white-space: nowrap;
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
            }
        }
    }
</style>