<script lang="ts">
    import { page } from "$app/stores";
    import { env } from "$env/dynamic/public";
    import { getUserByUsername } from "$lib/api/user";
    import { usernameRegex } from "$lib/patterns";

    let username = $page.url.searchParams.get("username");
    let usernameInput: HTMLInputElement;

    let usernameValid = true;
    let usernameErrorMsg: string;

    const onUsernameInput = async () => {
        await validateUsername();
    };

    const onFormSubmit = async () => {
        await validateUsername();

        if (!username || !usernameValid) {
            usernameInput.focus();
            return;
        }

        window.location.href = `${env.PUBLIC_API_BASE}/auth/register?username=${username}`;
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

    <form class="flex col" on:submit|preventDefault={onFormSubmit}>
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
