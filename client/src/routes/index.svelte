<script lang="ts">
    import PasteOptions from "../components/paste/PasteOptions.svelte";

    let expiresIn = "never";
</script>

<div class="title-input flex sm-row">
    <label class="hidden" for="paste-title">paste title</label>
    <input type="text" placeholder="title" id="paste-title" name="paste-title" />

    <button>expires in: {expiresIn}</button>
</div>

<div class="code-editor">
    <!-- TODO: using a simple text area just for testing -->
    <textarea name="paste-conents" id="paste-contents" rows="20" placeholder="paste contents" spellcheck="false"></textarea>
</div>

<div class="paste-options">
    <PasteOptions />
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

    .code-editor {
        margin-top: 1rem;
        
        textarea {
            width: 100%;
            resize: vertical;
        }
    }

    .paste-options {
        margin-top: 1rem;
    }
</style>