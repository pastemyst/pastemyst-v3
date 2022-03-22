<script lang="ts">
    import { page } from "$app/stores";
    import { getUser } from "../api/user";

    let username = $page.url.searchParams.get("username");
    let usernameTaken = false;

    const onUsernameInput = async () => {
        usernameTaken = (await getUser(username) !== null);
    };
</script>

<section>
    <h2>create a new account</h2>

    <form class="flex col">
        <label for="username">username:<span class="required">*</span></label>
        <input required type="text" name="username" id="username" placeholder="username..." bind:value={username} on:input={onUsernameInput} />

        <button type="submit" class="btn-main">create account</button>
    </form>
</section>

<style langs="scss">
    label {
        line-height: 2rem;
    }

    input,
    button {
        width: 50%;
    }

    button {
        margin-top: 2rem;
    }
</style>
