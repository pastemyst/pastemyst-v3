<script lang="ts">
    import { page } from "$app/stores";
    import { goto } from "$app/navigation";
    import { deleteCookie, setCookie } from "$lib/util/cookies";
    import { createAccount } from "$lib/api/auth";
    import { getUser } from "$lib/api/user";

    let username = $page.url.searchParams.get("username");
    let usernameInput: HTMLInputElement;

    let usernameValid = true;
    let usernameErrorMsg: string;

    const usernameRegex = /^[\w.-]+$/m;

    let createAccountError = false;

    const onUsernameInput = async () => {
        await validateUsername();
    };

    const onFormSubmit = async () => {
        await validateUsername();

        if (!usernameValid) {
            usernameInput.focus();
            return;
        }

        const token = await createAccount(username);

        if (token === null) {
            createAccountError = true;
            return;
        }

        setCookie("pastemyst", token, 30);
        deleteCookie("pastemyst-registration");

        goto("/");
    };

    const validateUsername = async () => {
        if ((await getUser(username)) !== null) {
            usernameValid = false;
            usernameErrorMsg = "this username is already taken";
        } else if (username.length === 0) {
            usernameValid = false;
            usernameErrorMsg = "the username can't be empty";
        } else if (!usernameRegex.test(username)) {
            usernameValid = false;
            usernameErrorMsg = "the username contains invalid symbols";
        } else {
            usernameValid = true;
            usernameErrorMsg = "";
        }
    };
</script>

<section class="centered">
    <h2>create a new account</h2>

    <p>
        the username has to be unique, it can contain alphanumeric characters, and only the symbols:
        <code>.</code>, <code>-</code>, <code>_</code>
    </p>

    <form class="flex col" on:submit|preventDefault={onFormSubmit}>
        {#if createAccountError}
            <p class="error-message">
                There was an issue creating the account. Please try again. If the issue persists
                please <a href="/contact">contact us</a>.
            </p>
        {/if}

        <label for="username">
            username:<span class="required">*</span>

            {#if !usernameValid}
                <span class="error">{usernameErrorMsg}</span>
            {/if}
        </label>
        <input
            required
            type="text"
            name="username"
            id="username"
            placeholder="username..."
            bind:value={username}
            bind:this={usernameInput}
            on:input={onUsernameInput}
            maxlength="20"
            minlength="1"
        />

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

    button {
        margin-top: 2rem;
    }

    label,
    input,
    button {
        width: 100%;
    }
</style>
