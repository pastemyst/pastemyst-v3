<script lang="ts">
    import { createAccount } from "$lib/api/auth";
    import { getUserByUsername } from "$lib/api/user";
    import { usernameRegex } from "$lib/patterns";
    import { page } from "$app/state";

    let username = $state(page.url.searchParams.get("username"));
    let usernameInput: HTMLInputElement;

    let usernameValid = $state(true);
    let usernameErrorMsg: string = $state("");

    let createAccountErrorMsg: string | null = $state(null);

    const onUsernameInput = async () => {
        await validateUsername();
    };

    const onFormSubmit = async (event: SubmitEvent) => {
        event.preventDefault();

        await validateUsername();

        if (!username || !usernameValid) {
            usernameInput.focus();
            return;
        }

        createAccountErrorMsg = await createAccount(username);

        if (createAccountErrorMsg) return;

        // not using goto("/") becase the page needs to get refreshed
        // so the current account gets updated
        window.location.href = "/";
    };

    const validateUsername = async () => {
        if (!username) {
            usernameValid = false;
            usernameErrorMsg = "invalid username";
            return;
        }

        if ((await getUserByUsername(username)) !== null) {
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

    <form class="flex col" onsubmit={onFormSubmit}>
        {#if createAccountErrorMsg}
            <p class="error-message">
                There was an issue creating the account. Please try again. If the issue persists
                please <a href="/contact">contact us</a>. Message: {createAccountErrorMsg}
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
            oninput={onUsernameInput}
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
            color: var(--color-danger);
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
