<script lang="ts">
    import { page } from "$app/stores";
    import { getUser } from "../api/user";

    let username = $page.url.searchParams.get("username");
    let usernameInput: HTMLInputElement;
    let usernameTaken = false;

    const onFormSubmit = async () => {
        usernameTaken = (await getUser(username) !== null);

        if (usernameTaken) usernameInput.focus();
    };
</script>

<section>
    <h2>create a new account</h2>

    <form class="flex col" on:submit|preventDefault={onFormSubmit}>
        <label for="username">
            username:<span class="required">*</span>

            {#if usernameTaken}
                <span class="error">username is taken</span>
            {/if}
        </label>
        <input required type="text" name="username" id="username" placeholder="username..." bind:value={username} bind:this={usernameInput} />

        <button type="submit" class="btn-main">create account</button>
    </form>
</section>

<style lang="scss">
    label {
        line-height: 2rem;

        .error {
            color: $color-red;
            font-weight: normal;
        }
    }

    input,
    button {
        width: 50%;
    }

    button {
        margin-top: 2rem;
    }

    @media screen and (max-width: $break-med) {
        input,
        button {
            width: 100%;
        }
    }
</style>
